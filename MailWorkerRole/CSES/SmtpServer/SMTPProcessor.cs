using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using MailWorkerRole.CSES.Common;

namespace MailWorkerRole.CSES.SmtpServer
{
    /// <summary>
	/// SMTPProcessor handles a single SMTP client connection.  This
	/// class provides an implementation of the RFC821 specification.
	/// </summary>
	/// <remarks>
	/// 	Created by: Eric Daugherty
	/// </remarks>
	public class SMTPProcessor
	{
        // Command codes
		/// <summary>HELO Command</summary>
		public const int COMMAND_HELO = 0;
		/// <summary>RSET Command</summary>
	    public const int COMMAND_RSET = 1;
		/// <summary>NOOP Command</summary>
	    public const int COMMAND_NOOP = 2;
		/// <summary>QUIT Command</summary>
	    public const int COMMAND_QUIT = 3;
		/// <summary>MAIL FROM Command</summary>
	    public const int COMMAND_MAIL = 4;
		/// <summary>RCPT TO Command</summary>
	    public const int COMMAND_RCPT = 5;
		/// <summary>DATA Comand</summary>
	    public const int COMMAND_DATA = 6;

		// Messages
		private const string MESSAGE_DEFAULT_WELCOME = "220 {0} Welcome to Eric Daugherty's C# SMTP Server.";
		private const string MESSAGE_DEFAULT_HELO_RESPONSE = "250 {0}";
		private const string MESSAGE_OK = "250 OK";
		private const string MESSAGE_START_DATA = "354 Start mail input; end with <CRLF>.<CRLF>";
		private const string MESSAGE_GOODBYE = "221 Goodbye.";

		private const string MESSAGE_UNKNOWN_COMMAND = "500 Command Unrecognized.";
		private const string MESSAGE_INVALID_COMMAND_ORDER = "503 Command not allowed here.";
		private const string MESSAGE_INVALID_ARGUMENT_COUNT = "501 Incorrect number of arguments.";
		
		private const string MESSAGE_INVALID_ADDRESS = "451 Address is invalid.";
		private const string MESSAGE_UNKNOWN_USER = "550 User does not exist.";
		
		private const string MESSAGE_SYSTEM_ERROR = "554 Transaction failed.";
		
		// Regular Expressions
		private static readonly Regex ADDRESS_REGEX = new Regex( "<.+@.+>", RegexOptions.IgnoreCase );

        /// <summary>
		/// Every connection will be assigned a unique id to 
		/// provide consistent log output and tracking.
		/// </summary>
		private long connection_id;
		
		/// <summary>Determines which recipients to accept for delivery.</summary>
		private readonly IRecipientFilter recipient_filter;
		
		/// <summary>Incoming Message spool</summary>
		private readonly IMessageSpool message_spool;

		/// <summary>Domain name for this server.</summary>
		private string domain;

		/// <summary>The message to display to the client when they first connect.</summary>
		private string welcome_message;
		
		/// <summary>The response to the HELO command.</summary>
		private string helo_response;

		/// <summary>
		/// Initializes the SMTPProcessor with the appropriate 
		/// interface implementations.  This allows the relay and
		/// delivery behaviour of the SMTPProcessor to be defined
		/// by the specific server.
		/// </summary>
		/// <param name="domain">
		/// The domain name this server handles mail for.  This does not have to
		/// be a valid domain name, but it will be included in the Welcome Message
		/// and HELO response.
		/// </param>
		/// <param name="recipientFilter">
		/// The IRecipientFilter implementation is responsible for 
		/// filtering the recipient addresses to determine which ones
		/// to accept for delivery.
		/// </param>
		/// <param name="messageSpool">
		/// The IMessageSpool implementation is responsible for 
		/// spooling the inbound message once it has been recieved from the sender.
		/// </param>
		public SMTPProcessor( string domain, IRecipientFilter recipientFilter, IMessageSpool messageSpool )
		{
			Initialize( domain );
						
			recipient_filter = recipientFilter;
			message_spool = messageSpool;
		}
		
		/// <summary>
		/// Provides common initialization logic for the constructors.
		/// </summary>
		private void Initialize( string domain )
		{
			// Initialize the connectionId counter
			connection_id = 1;
			
			this.domain = domain;
			
			// Initialize default messages
			welcome_message = String.Format( MESSAGE_DEFAULT_WELCOME, domain );
			helo_response = String.Format( MESSAGE_DEFAULT_HELO_RESPONSE, domain );		
		}

        /// <summary>
		/// Returns the welcome message to display to new client connections.
		/// This method can be overridden to allow for user defined welcome messages.
		/// Please refer to RFC 821 for the rules on acceptable welcome messages.
		/// </summary>
		public virtual string WelcomeMessage
		{
			get
			{
				return welcome_message;
			}
			set
			{
				welcome_message = String.Format( value, domain );
			}
		}
		
		/// <summary>
		/// The response to the HELO command.  This response should
		/// include the local server's domain name.  Please refer to RFC 821
		/// for more details.
		/// </summary>
		public virtual string HeloResponse
		{
			get
			{
				return helo_response;
			}
			set
			{
				helo_response = String.Format( value, domain );
			}
		}

        /// <summary>
		/// ProcessConnection handles a connected TCP Client
		/// and performs all necessary interaction with this
		/// client to comply with RFC821.  This method is thread 
		/// safe.
		/// </summary>
		public void ProcessConnection( Socket socket )
		{
			long current_connection_id;
			// Really only need to lock on the long, but that is not
			// allowed.  Is there a better way to do this?
			lock( this )
			{
				current_connection_id = connection_id++;
			}
			
			var context = new SMTPContext( current_connection_id, socket );
			try 
			{
				SendWelcomeMessage( context );
				ProcessCommands( context );
			}
			catch( Exception exception )
			{
			    Trace.TraceError("Connection {0}: Error: {1}", context.ConnectionId, exception);
            }
		}

        /// <summary>
		/// Sends the welcome greeting to the client.
		/// </summary>
		private void SendWelcomeMessage( SMTPContext context )
		{
			context.WriteLine( WelcomeMessage );
		}
		
		/// <summary>
		/// Handles the command input from the client.  This
		/// message returns when the client issues the quit command.
		/// </summary>
		private void ProcessCommands( SMTPContext context )
		{
		    // Loop until the client quits.
			var is_running = true;
			while( is_running )
			{
				try
				{
					var input_line = context.ReadLine();
					if( input_line == null )
					{
						is_running = false;
						context.Close();
						continue;
					}

					Trace.TraceInformation( "ProcessCommands Read: " + input_line );
					var inputs = input_line.Split( " ".ToCharArray() );
					
					switch( inputs[0].ToLower() )
					{
						case "helo":
							Helo( context, inputs );
							break;
						case "rset":
							Rset( context );
							break;
						case "noop":
							context.WriteLine( MESSAGE_OK );
							break;
						case "quit":
							is_running = false;
							context.WriteLine( MESSAGE_GOODBYE );
							context.Close();
							break;
						case "mail":
							if( inputs[1].ToLower().StartsWith( "from" ) )
							{
								Mail( context, input_line.Substring( input_line.IndexOf(" ", StringComparison.InvariantCulture) ) );
								break;
							}
							context.WriteLine( MESSAGE_UNKNOWN_COMMAND );
							break;
						case "rcpt":
							if( inputs[1].ToLower().StartsWith( "to" ) ) 							
							{
								Rcpt( context, input_line.Substring( input_line.IndexOf( " ", StringComparison.InvariantCulture ) ) );
								break;
							}
							context.WriteLine( MESSAGE_UNKNOWN_COMMAND );
							break;
						case "data":
							Data( context );
							break;
						default:
							context.WriteLine( MESSAGE_UNKNOWN_COMMAND );
							break;
					}				
				}
				catch( Exception exception )
				{
					Trace.TraceError( "Connection {0}: Exception occured while processing commands: {1}", context.ConnectionId, exception );
					context.WriteLine( MESSAGE_SYSTEM_ERROR );
				}
			}
		}

		/// <summary>
		/// Handles the HELO command.
		/// </summary>
		private void Helo( SMTPContext context, IList<string> inputs )
		{
		    if (inputs == null)
                throw new ArgumentNullException("inputs");

		    if( context.LastCommand == -1 )
			{
				if( inputs.Count == 2 )
				{
					context.ClientDomain = inputs[1];
					context.LastCommand = COMMAND_HELO;
					context.WriteLine( HeloResponse );				
				}
				else
				{
					context.WriteLine( MESSAGE_INVALID_ARGUMENT_COUNT );
				}
			}
			else
			{
				context.WriteLine( MESSAGE_INVALID_COMMAND_ORDER );
			}
		}

        /// <summary>
		/// Reset the connection state.
		/// </summary>
		private static void Rset( SMTPContext context )
		{
			if( context.LastCommand != -1 )
			{
				// Dump the message and reset the context.
				context.Reset();
				context.WriteLine( MESSAGE_OK );
			}
			else
			{
				context.WriteLine( MESSAGE_INVALID_COMMAND_ORDER );
			}
		}
		
		/// <summary>
		/// Handle the MAIL FROM:&lt;address&gt; command.
		/// </summary>
		private void Mail( SMTPContext context, string argument )
		{
			var address_valid = false;
			if( context.LastCommand == COMMAND_HELO )
			{
				var address = ParseAddress( argument );
				if( address != null )
				{
					try
					{
						var email_address = new EmailAddress( address );
						context.Message.FromAddress = email_address;
						context.LastCommand = COMMAND_MAIL;
						address_valid = true;
						context.WriteLine( MESSAGE_OK );
						Trace.TraceInformation("Connection {0}: MailFrom address: {1} accepted.", context.ConnectionId, address);
					}
					catch( InvalidEmailAddressException )
					{
						// This is fine, just fall through.
					}
				}
				
				// If the address is invalid, inform the client.
				if( !address_valid )
				{
					Trace.TraceWarning("Connection {0}: MailFrom argument: {1} rejected.  Should be from:<username@domain.com>", context.ConnectionId, argument);
					context.WriteLine( MESSAGE_INVALID_ADDRESS );
				}
			}
			else
			{
				context.WriteLine( MESSAGE_INVALID_COMMAND_ORDER );
			}
		}
		
		/// <summary>
		/// Handle the RCPT TO:&lt;address&gt; command.
		/// </summary>
		private void Rcpt( SMTPContext context, string argument )
		{
			if( context.LastCommand == COMMAND_MAIL || context.LastCommand == COMMAND_RCPT )
			{				
				var address = ParseAddress( argument );
				if( address != null )
				{
					try
					{
						var emailAddress = new EmailAddress( address );
						
						// Check to make sure we want to accept this message.
						if( recipient_filter.AcceptRecipient( context, emailAddress ) )
						{						
							context.Message.AddToAddress( emailAddress );
							context.LastCommand = COMMAND_RCPT;							
							context.WriteLine( MESSAGE_OK );
							Trace.TraceInformation("Connection {0}: RcptTo address: {1} accepted.", context.ConnectionId, address);
						}
						else
						{
							context.WriteLine( MESSAGE_UNKNOWN_USER );
                            Trace.TraceInformation("Connection {0}: RcptTo address: {1} rejected.  Did not pass Address Filter.", context.ConnectionId, address);
						}
					}
					catch( InvalidEmailAddressException )
					{
                        Trace.TraceWarning("Connection {0}: RcptTo argument: {1} rejected.  Should be from:<username@domain.com>", context.ConnectionId, argument);
						context.WriteLine( MESSAGE_INVALID_ADDRESS );
					}
				}
				else
				{
					Trace.TraceInformation("Connection {0}: RcptTo argument: {1} rejected.  Should be from:<username@domain.com>", context.ConnectionId, argument);
					context.WriteLine( MESSAGE_INVALID_ADDRESS );
				}
			}
			else
			{
				context.WriteLine( MESSAGE_INVALID_COMMAND_ORDER );
			}
		}
		
		private void Data( SMTPContext context )
		{
			context.WriteLine( MESSAGE_START_DATA );
			
			var message = context.Message;
			var client_end_point = (IPEndPoint) context.Socket.RemoteEndPoint;
			var header = new StringBuilder();
			header.AppendLine( String.Format( "Received: from {0} ({0} [{1}])", context.ClientDomain, client_end_point.Address ) );
			header.AppendLine( String.Format( "     by {0} (Eric Daugherty's C# Email Server)", domain ) );
			header.AppendLine( "     " + DateTime.Now );
			
			message.AddData( header.ToString() );
			
			var line = context.ReadLine();
			while( !line.Equals( "." ) )
			{
				message.AddData( line );
				message.AddData( "\r\n" );
				line = context.ReadLine();
			}
			
			// Spool the message
			message_spool.SpoolMessage( message );
			context.WriteLine( MESSAGE_OK );
			
			// Reset the connection.
			context.Reset();
		}

        /// <summary>
		/// Parses a valid email address out of the input string and return it.
		/// Null is returned if no address is found.
		/// </summary>
		private string ParseAddress( string input )
		{
			var match = ADDRESS_REGEX.Match( input );
            if( match.Success )
			{
				string matchText = match.Value;
				
				// Trim off the :< chars
				matchText = matchText.Remove( 0, 1 );
				// trim off the . char.
				matchText = matchText.Remove( matchText.Length - 1, 1 );
				
				return matchText;
			}
			return null;
		}
	}
}
