using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace MailWorkerRole.CSES.SmtpServer
{
    /// <summary>
	/// Maintains the current state for a SMTP client connection.
	/// </summary>
	/// <remarks>
	/// This class is similar to a HTTP Session.  It is used to maintain all
	/// the state information about the current connection.
	/// </remarks>
	public class SMTPContext : object
	{
        private const string EOL = "\r\n";

        /// <summary>The unique ID assigned to this connection</summary>
		private readonly long connection_id;
		
		/// <summary>The socket to the client.</summary>
		private readonly Socket socket;
		
		/// <summary>Last successful command received.</summary>
		private int last_command;

        /// <summary>The incoming message.</summary>
		private SMTPMessage message;
		
		/// <summary>Encoding to use to send/receive data from the socket.</summary>
		private readonly Encoding encoding;
		
		/// <summary>
		/// It is possible that more than one line will be in
		/// the queue at any one time, so we need to store any input
		/// that has been read from the socket but not requested by the
		/// ReadLine command yet.
		/// </summary>
		private StringBuilder input_buffer;

        /// <summary>
		/// Initialize this context for a given socket connection.
		/// </summary>
		public SMTPContext( long connectionId, Socket socket )
		{
			Trace.TraceInformation("Connection {0}: New connection from client {1}", connectionId, socket.RemoteEndPoint);
			
			connection_id = connectionId;
			last_command = -1;
			this.socket = socket;
			message = new SMTPMessage();
			
			// Set the encoding to ASCII.  
			encoding = Encoding.ASCII;
			
			// Initialize the input buffer
			input_buffer = new StringBuilder();
		}

        /// <summary>
		/// The unique connection id.
		/// </summary>
		public long ConnectionId
		{
			get
			{
				return connection_id;
			}
		}
		
		/// <summary>
		/// Last successful command received.
		/// </summary>
		public int LastCommand
		{
			get
			{
				return last_command;
			}
			set
			{
				last_command = value;
			}
		}

        /// <summary>
        /// The client domain, as specified by the helo command.
        /// </summary>
        public string ClientDomain { get; set; }

        /// <summary>
		/// The Socket that is connected to the client.
		/// </summary>
		public Socket Socket
		{
			get
			{
				return socket;
			}
		}
		
		/// <summary>
		/// The SMTPMessage that is currently being received.
		/// </summary>
		public SMTPMessage Message
		{
			get
			{
				return message;
			}
			set
			{
				message = value;
			}
		}

        #region Public Methods
		
		/// <summary>
		/// Writes the string to the socket as an entire line.  This
		/// method will append the end of line characters, so the data
		/// parameter should not contain them.
		/// </summary>
		/// <param name="data">The data to write the the client.</param>
		public void WriteLine( string data )
		{
			Trace.TraceInformation("Connection {0}: Wrote Line: {1}", connection_id, data);
			socket.Send( encoding.GetBytes( data + EOL ) );
		}
		
		/// <summary>
		/// Reads an entire line from the socket.  This method
		/// will block until an entire line has been read.
		/// </summary>
		public String ReadLine()
		{
			// If we already buffered another line, just return
			// from the buffer.			
			var output = ReadBuffer();
			if( output != null )
			{
				return output;
			}
						
			// Otherwise, read more input.
			var byteBuffer = new byte[80];

		    // Read from the socket until an entire line has been read.			
			do
			{
				// Read the input data.
				var count = socket.Receive( byteBuffer );
				if( count == 0 )
				{
					Trace.TraceWarning( "Socket closed before end of line received.");
					return null;
				}

				input_buffer.Append( encoding.GetString( byteBuffer, 0, count ) );				
				Trace.TraceInformation("Connection {0}: Read: {1}", connection_id, input_buffer);
			}
			while( (output = ReadBuffer()) == null );
			
			// IO Log statement is in ReadBuffer...
			
			return output;
		}
		
		/// <summary>
		/// Resets this context for a new message
		/// </summary>
		public void Reset()
		{
			Trace.TraceInformation("Connection {0}: Reset", connection_id);
			message = new SMTPMessage();
			last_command = SMTPProcessor.COMMAND_HELO;
		}
		
		/// <summary>
		/// Closes the socket connection to the client and performs any cleanup.
		/// </summary>
		public void Close()
		{
			socket.Close();
		}
		
		#endregion

        /// <summary>
		/// Helper method that returns the first full line in
		/// the input buffer, or null if there is no line in the buffer.
		/// If a line is found, it will also be removed from the buffer.
		/// </summary>
		private string ReadBuffer()
		{
			// If the buffer has data, check for a full line.
			if( input_buffer.Length > 0 )				
			{
				var buffer = input_buffer.ToString();
				var eolIndex = buffer.IndexOf( EOL, StringComparison.InvariantCulture );
				if( eolIndex != -1 )
				{
					var output = buffer.Substring( 0, eolIndex );
					input_buffer = new StringBuilder( buffer.Substring( eolIndex + 2 ) );
					Trace.TraceInformation("Connection {0}: Read Line: {1}", connection_id, output);
					return output;
				}				
			}
			return null;
		}
	}
}
