using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MailWorkerRole.CSES.Implementation;
using MailWorkerRole.CSES.SmtpServer;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace MailWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellation_token_source = new CancellationTokenSource();
        private readonly ManualResetEvent run_complete_event = new ManualResetEvent(false);
        private readonly ManualResetEvent connection_wait_handle = new ManualResetEvent(false);
        private TcpListener listener;
        private SMTPProcessor processor;

        public override void Run()
        {
            Trace.TraceInformation("MailWorkerRole is running");

            try
            {
                while (!cancellation_token_source.IsCancellationRequested)
                {
                    connection_wait_handle.Reset();
                    listener.BeginAcceptSocket(ar =>
                    {
                        if (listener.Server != null && listener.Server.IsBound)
                            processor.ProcessConnection(listener.EndAcceptSocket(ar));

                        connection_wait_handle.Set();
                    }, null);
                    connection_wait_handle.WaitOne();
                }
            }
            catch (SocketException e)
            {
                Trace.TraceError("Socket exception: " + e);
            }
            finally
            {
                run_complete_event.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            var result = base.OnStart();
            try
            {
                processor = new SMTPProcessor(RoleEnvironment.GetConfigurationSettingValue("DomainName"), new RecipientFilter(), new MessageSpool());
                var endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["email"].IPEndpoint;
                listener = new TcpListener(endpoint) { ExclusiveAddressUse = false };
                listener.Start();

                Trace.TraceInformation("MailWorkerRole has been started");
            }
            catch (SocketException)
            {
                Trace.TraceError("MailWorkerRole could not start.");
            }

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("MailWorkerRole is stopping");

            cancellation_token_source.Cancel();
            listener.Stop();
            run_complete_event.WaitOne();

            base.OnStop();

            Trace.TraceInformation("MailWorkerRole has stopped");
        }
    }
}
