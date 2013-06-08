using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OleDb;
using System.Configuration;
using System.Data;
using CDI.Encripcion;
using System.IO;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

/// <summary>
/// Clase para realizar el acceso a la BD, se definen las conexion y metodos para realizar DML
/// </summary>
/// 

namespace PGENL0001
{
    public class AccesoDatos
    {
        public AccesoDatos()
        {
            //constructor code here
        }
        /// <summary>
        /// Metodo para armar la Cadena de Conexion a DB2 / AS400
        /// </summary>
        /// <returns>devuelve un String con la Cadena de Conexion</returns>
        private String getConexion()
        {
            string claveEncriptada, claveDecriptada, conexion;

            claveEncriptada = ConfigurationManager.AppSettings["ClaveCadenaConexion"].ToString();
            claveDecriptada = RijndaelEncryptor.Decrypt(claveEncriptada, "informacion");

            conexion = ConfigurationManager.ConnectionStrings["a400Connect"].ConnectionString;
            conexion = string.Format(conexion, claveDecriptada);
            return conexion;
        }
        /// <summary>
        /// Metodo para armar la cadena de Conexion a BD de MySql
        /// </summary>
        /// <returns>devuelve un String con la Cadena de Conexion</returns>
        private String getConexion_MySql()
        {
            string claveEncriptada, claveDecriptada, conexion;
            claveEncriptada = ConfigurationManager.AppSettings["MySqlConexion"].ToString();
            claveDecriptada = RijndaelEncryptor.Decrypt(claveEncriptada, "MetaData");

            conexion = ConfigurationManager.ConnectionStrings["MySqlConn"].ConnectionString;
            conexion = string.Format(conexion, claveDecriptada);
            return conexion;
        }

        /// <summary>
        /// Metodo para armar la Cadena de Conexion a BD usando ODBC
        /// </summary>
        /// <returns>devuelve un string con la cadena de conexion</returns>
        public String getConexion_Odbc()
        {
            string claveEncriptada, claveDecriptada, conexion;

            claveEncriptada = ConfigurationManager.AppSettings["ClaveCadenaConexion"].ToString();
            claveDecriptada = RijndaelEncryptor.Decrypt(claveEncriptada, "informacion");

            conexion = ConfigurationManager.ConnectionStrings["ODBC_Connect"].ConnectionString;
            conexion = string.Format(conexion, claveDecriptada);
            return conexion;
        }
        /// <summary>
        /// Metodo crear un nuevo OleDbCommand de DB2
        /// </summary>
        /// <returns>Devuelve una nueva Instancia de un Commando SQL</returns>
        private OleDbCommand CrearComando()
        {
            OleDbConnection _conexion = new OleDbConnection();
            _conexion.ConnectionString = getConexion();
            OleDbCommand _comando = _conexion.CreateCommand();
            _comando.CommandType = CommandType.Text;
            return _comando;
        }
        /// <summary>
        /// Metodo para poder ejecutar un comando DML
        /// </summary>
        /// <param name="comando">Comando Insert, Update, Delete</param>
        /// <returns>1 si es Satisfactorio y -1 si hubo fallo</returns>
        private int EjecutaDML(OleDbCommand comando)
        {
            int resultado;
            try
            {
                comando.Connection.Open();
                resultado = comando.ExecuteNonQuery();

            }
            catch (Exception err)
            {
                resultado = -1;
                throw err;
            }
            finally
            {
                comando.Connection.Close();
            }
            return resultado;
        }
        /// <summary>
        /// Metodo para ejecutar un Select en SQL
        /// </summary>
        /// <param name="comando">Comando sql con la query</param>
        /// <returns>Un DataTable con los resultados consultados</returns>
        private DataTable EjecutarComandoSelect(OleDbCommand comando)
        {
            DataTable _tabla;
            try
            {
                comando.Connection.Open();
                OleDbDataReader _lector = comando.ExecuteReader();
                _tabla = new DataTable();
                _tabla.Load(_lector);
                _lector.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                comando.Connection.Close();
            }
            return _tabla;
        }
        /// <summary>
        /// Metodo para ejecutar el comando DML
        /// </summary>
        /// <param name="dml">String con el comando Insert, Update, Delete</param>
        /// <returns>Devuelve un 1 si es satisfactorio y un -1 si hubo fallo</returns>
        public int RealizaDml(string dml)
        {
            OleDbCommand _comando = CrearComando();
            _comando.CommandText = dml;
            return EjecutaDML(_comando);
        }
        /// <summary>
        /// Inserta archivo
        /// </summary>
        /// <param name="dml">instrucción insert, update</param>
        /// <param name="nombreParametro">nombre del parametro a ejecutar</param>
        /// <param name="contenido">bytes del archivo a insertar o a actualizar</param>
        /// <returns>devuelve 1 si es Satisfactorio o un -1 Si hubo error</returns>
         public int RealizaDml(string dml, string nombreParametro,byte[] contenido)
        {
            OleDbCommand _comando = CrearComando();
            
            _comando.CommandText = dml; 
            OleDbParameter param = new OleDbParameter(String.Format("@{0}",nombreParametro), OleDbType.Binary); 
            param.OleDbType = OleDbType.Binary; 
            param.Value = contenido; 
            param.Size = contenido.Length; 
            _comando.Parameters.Add(param); 
            
            return EjecutaDML(_comando);
        }
        /// <summary>
        /// Metodo para realizar querys en DB2
        /// </summary>
        /// <param name="consulta">string con la query a consultar</param>
        /// <returns>un DataTable con los resultados de la Query</returns>
        public DataTable RealizaConsulta(string consulta)
        {
            OleDbCommand _comando = CrearComando();
            _comando.CommandText = consulta;
            return EjecutarComandoSelect(_comando);
        }

        /// <summary>
        /// Metodo para realizar consultas a DB MySql
        /// </summary>
        /// <param name="consulta">String de la query</param>
        /// <returns>devuelve un DataTable con los resultados obtenidos</returns>
        public DataTable MySQL_RealizaConsulta(string consulta)
        {
            MySqlCommand _comando = CrearComando_MySQL();
            _comando.CommandText = consulta;
            return MySQL_EjecutarComandoSelect(_comando);
        }
        /// <summary>
        /// Metodo para realizar consultas a DB Sql Server
        /// </summary>
        /// <param name="consulta">String de la query</param>
        /// <returns>devuelve un DataTable con los resultados obtenidos</returns>
        public DataTable SQLSvr_RealizaConsulta(string consulta)
        {
            DataTable retorno = new DataTable();
            SqlConnection myConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["SQLEXPRESS"].ConnectionString);
            try
            {
                myConnection.Open();
                string qry = consulta;
                SqlDataAdapter da = new SqlDataAdapter(qry, myConnection);
                da.Fill(retorno);
            }

            catch (Exception err)
            {
                myConnection.Close();
                throw new Exception(err.Message); ;
            }
            finally
            {
                myConnection.Close();
            }
            return retorno;
            
        }
        /// <summary>
        /// Metodo para realizar operaciones DML en MySql
        /// </summary>
        /// <param name="dml">String con el comando sql de Insert, Update o Delete</param>
        /// <returns>devuelve 1 si es Satisfactorio o un -1 Si hubo error </returns>
        public int MySQL_RealizaDml(string dml)
        {
            MySqlCommand _comando = CrearComando_MySQL();
            _comando.CommandText = dml;
            return MySQL_EjecutaDML(_comando);
        }

        /// <summary>
        /// Inserta imagen
        /// </summary>
        /// <param name="dml">instrucción insert, update</param>
        /// <param name="nombreParametro">nombre del parametro a ejecutar</param>
        /// <param name="contenido">bytes del archivo a insertar o a actualizar</param>
        /// <returns>devuelve 1 si es Satisfactorio o un -1 Si hubo error</returns>
        public int MySQL_RealizaDml(string dml, string nombreParametro,byte[] contenido)
        {
            MySqlCommand _comando = CrearComando_MySQL();
            _comando.CommandText = dml;
            MySqlParameter parametroLongBlob = new MySqlParameter(String.Format("?{0}",nombreParametro), MySqlDbType.LongBlob);
            parametroLongBlob.Size = contenido.Length;
            parametroLongBlob.Value = contenido;
            _comando.Parameters.Add(parametroLongBlob);
            return MySQL_EjecutaDML(_comando);
        }

        /// <summary>
        /// Método auxiliar para ejecutar comando en MySql
        /// </summary>
        /// <param name="comando">Comando SQL armado</param>
        /// <returns>un DataTable con los resultados Obtenidos</returns>
        private DataTable MySQL_EjecutarComandoSelect(MySqlCommand comando)
        {
            DataTable _tabla;
            try
            {
                comando.Connection.Open();
                MySqlDataReader _lector = comando.ExecuteReader();
                _tabla = new DataTable();
                _tabla.Load(_lector);
                _lector.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                comando.Connection.Close();
            }
            return _tabla;
        }       

        /// <summary>
        /// Metodo para crear el comando en MySql
        /// </summary>
        /// <returns>Devuelve una nueva instancia de un comando en MySql</returns>
        private MySqlCommand CrearComando_MySQL()
        {
            MySqlConnection _conexion = new MySqlConnection();
            _conexion.ConnectionString = getConexion_MySql();
            MySqlCommand _comando = _conexion.CreateCommand();
            _comando.CommandType = CommandType.Text;
            return _comando;
        }
        /// <summary>
        /// Metodo para ejecutar comando DML en MySql
        /// </summary>
        /// <param name="comando">Comando de MySql</param>
        /// <returns>Devuelve un 1 si es satisfactorio y un -e1 si hubo error</returns>
        private int MySQL_EjecutaDML(MySqlCommand comando)
        {
            int resultado;
            try
            {
                comando.Connection.Open();
                resultado = comando.ExecuteNonQuery();

            }
            catch (Exception err)
            {
                resultado = -1;
                throw err;
            }
            finally
            {
                comando.Connection.Close();
            }
            return resultado;
        }
      

    }
}
