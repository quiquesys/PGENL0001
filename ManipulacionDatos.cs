using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Script.Serialization;
using System.Text;
using System.Globalization;
using System.IO;

/// <summary>
/// Descripción breve de ManipulacionDatos
/// </summary>
/// 
namespace PGENL0001
{
    public class ManipulacionDatos
    {
        // <summary>  
        /// Quotes the string.  
        /// </summary>  
        /// <param name="s">The s.</param>  

        /// <param name="sb">The sb.</param>  
        private void QuoteString(string s, StringBuilder sb)
        {
            QuoteString(s, '"', sb);
        }
        /// <summary>  
        /// Quotes the string.  
        /// </summary>  
        /// <param name="s">The s.</param>  
        /// <param name="quoteChar">The quote char.</param>  
        /// <param name="sb">The sb.</param>  
        private void QuoteString(string s, char quoteChar, StringBuilder sb)
        {
            if (s == null || (s.Length == 1 && s[0] == '\0'))
            {
                sb.Append(new String(quoteChar, 2));
                return;
            }

            char c;
            int len = s.Length;

            sb.EnsureCapacity(sb.Length + s.Length + 2);

            sb.Append(quoteChar);

            for (int i = 0; i < len; i++) { c = s[i]; switch (c) { case '\\': sb.Append("\\\\"); break; case '\b': sb.Append("\\b"); break; case '\t': sb.Append("\\t"); break; case '\r': sb.Append("\\r"); break; case '\n': sb.Append("\\n"); break; case '\f': sb.Append("\\f"); break; default: if (c < ' ') { sb.Append("\\u"); sb.Append(((int)c).ToString("x4", CultureInfo.InvariantCulture)); } else if (c == quoteChar) { sb.Append("\\"); sb.Append(c); } else { sb.Append(c); } break; } } sb.Append(quoteChar);
        }

        public void DTSerialize(object o, StringBuilder sb)
        {
            DataTable dt = o as DataTable;

            if (dt == null)
                throw new NotSupportedException();

            DataColumnCollection cols = dt.Columns;
            DataRowCollection rows = dt.Rows;

            bool b = true;

            sb.Append('[');

            foreach (DataRow row in rows)
            {
                if (b) { b = false; }
                else { sb.Append(","); }

                sb.Append('{');

                bool bc = true;

                foreach (DataColumn col in dt.Columns)
                {
                    if (bc) { bc = false; }
                    else { sb.Append(","); }

                    QuoteString(col.ColumnName, sb);
                    sb.Append(':');
                    JavaScriptSerializer JS = new JavaScriptSerializer();
                    JS.Serialize(row[col], sb);
                }

                sb.Append('}');
            }

            sb.Append(']');
        }
        /// <summary>
        /// Convierte en bytes un archivo
        /// </summary>
        /// <param name="fileDirectory">Directorio del archivo a convertir</param>
        /// <returns>Bytes del archivo convertido</returns>
        public byte[] ReadFileByteArray(string fileDirectory)
        {
            FileStream fs = new FileStream(fileDirectory, FileMode.Open, FileAccess.Read);
            byte[] fileData = new byte[fs.Length];
            fs.Read(fileData, 0, System.Convert.ToInt32(fs.Length));
            fs.Close();
            return fileData;
        } 
    }
}
