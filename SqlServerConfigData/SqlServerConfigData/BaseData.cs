using Microsoft.Extensions.Configuration;
using SqlServerConfigData.ConfigData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace SqlServerConfigData
{
    public abstract class BaseData
    {
        private readonly List<DbParameter> ListParameters;
        private static IConfiguration _configuration;

        public BaseData(IConfiguration configuration)
        {
            _configuration = configuration;
            ListParameters = new List<DbParameter>();
        }

        public static DataBase ObterBaseDados()
        {
            return (_configuration.GetSection("AppSettings").GetSection("Ambiente").Value.ToUpper()) switch
            {
                "PRD" => DataBase.GetDataBase(_configuration),
                "HOM" => DataBase.GetDataBaseHom(_configuration),
                _ => DataBase.GetDataBaseDes(_configuration)
            };
        }

        /// <summary>
        /// Metodo que retorna a conexão da base de dados
        /// </summary>
        /// <returns></returns>
        public DataBase Conexao()
        {
            DataBase database;
            try
            {
                database = ObterBaseDados();

                return database;
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return null;
        }

        /// <summary>
        /// Metodo utilizado para criar os parametros a serem inseridos na base de dados.
        /// </summary>
        /// <param name="nome"></param> 
        /// <param name="tipo"></param>
        /// <param name="valor"></param>
        public void CriarParametrosEntrada(string nome, DbType tipo, object valor)
        {
            SqlParameter sqlParameter = new SqlParameter
            {
                ParameterName = nome,
                DbType = tipo,
                Value = valor ?? DBNull.Value,
                Direction = ParameterDirection.Input
            };

            ListParameters.Add(sqlParameter);
        }

        /// <summary>
        /// Cria a lista de parametros a ser incluido na base de dados
        /// </summary>
        /// <param name="par"></param>
        public void Parametros(ExecutionParameter executionParameter)
        {
            try
            {
                foreach (var item in ListParameters)
                {
                    executionParameter.Add(item.ParameterName, item.Direction, item.DbType, item.Value);
                }

                ListParameters.Clear();
            }
            catch (Exception)
            {
                ListParameters.Clear();
            }
        }
    }
}
