using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace MailWorkerRole.CSES.SmtpServer
{
	/// <summary>
	/// Stores a single part of a multipart message.
	/// </summary>
	public class SMTPMessagePart
	{
	    private static readonly string DOUBLE_NEWLINE = Environment.NewLine + Environment.NewLine;

		private readonly string header_data = String.Empty;
		private readonly string body_data = String.Empty;
	    private Hashtable header_fields;

	    /// <summary>
		/// Creates a new message part.  The input string should be the body of 
		/// the attachment, without the "------=_NextPart" separator strings.
		/// The last 4 characters of the data will be "\r\n\r\n".
		/// </summary>
		public SMTPMessagePart( string data )
		{
			var parts = Regex.Split( data, DOUBLE_NEWLINE );

			header_data = parts[0] + DOUBLE_NEWLINE;
			body_data = parts[1] + DOUBLE_NEWLINE;					
		}

	    /// <summary>
		/// A hash table of all the Headers in the email message.  They keys
		/// are the header names, and the values are the assoicated values, including
		/// any sub key/value pairs is the header.
		/// </summary>
		public Hashtable Headers
		{
	        get
	        {
	            return header_fields ?? (header_fields = SMTPMessage.ParseHeaders(header_data));
	        }
		}
		/// <summary>
		/// The raw text that represents the header of the mime part.
		/// </summary>
		public string HeaderData
		{
			get { return header_data; }
		}

		/// <summary>
		/// The raw text that represents the actual mime part.
		/// </summary>
		public string BodyData
		{
			get { return body_data; }
		}
	}
}
