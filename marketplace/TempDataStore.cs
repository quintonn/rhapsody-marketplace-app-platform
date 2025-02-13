﻿using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using QBic.Core.Data;
using QBic.Core.Utilities;
using System;
using System.Configuration;
using System.Data;
using System.IO;

namespace Marketplace
{
    public class TempDataStore
    {
        // need to prevent global access to data store for save/update/delete. 
        // this needs to go through data service so auditing etc can be done.
        // maybe also for retrievals, not sure if that should be audited too. - maybe make this configurable. could impact performance
        // to prevent performace impact, this might have to be handled by a background thread or something like that.

        //for now just use nhibernate                                                                            

        //-- Long term maybe make my own query language??
        private static TempDataStore _instance { get; set; }

        private static bool UpdateDatabase { get; set; }
        private static bool ShowSql { get; set; }
        public static string ProviderName { get; set; }
        private static IConfiguration Config { get; set; }

        private TempDataStore(bool updateDatabase, bool showSql)
        {
            UpdateDatabase = updateDatabase;
            ShowSql = showSql;
            init();
        }

        public static TempDataStore GetInstance(bool updateDatabase, bool showSql, IConfiguration config, IServiceCollection serviceProvider = null)
        {
            if (_instance == null)
            {
                Config = config;
                _instance = new TempDataStore(updateDatabase, showSql);
                if (serviceProvider != null)
                {
                    serviceProvider.AddTransient<ISessionFactory>((x) =>
                    {
                        return Store;
                    });
                }
            }
            return _instance;
        }

        private static ISessionFactory Store;
        private static ISessionFactory AuditStore;
        private static NHibernate.Cfg.Configuration Configuration;

        //public string GetTableName(Type type)
        //{
        //    var tableName = Configuration.GetClassMapping(type).Table.Name;
        //    return tableName;
        //}

        private void init()
        {
            Store = CreateSessionFactory();
            AuditStore = CreateAuditSessionFactory();

            if (UpdateDatabase)
            {
                new SchemaUpdate(Configuration).Execute(false, UpdateDatabase);
            }
        }

        private ISessionFactory CreateSessionFactory()
        {
            var container = new FluentMappingsContainer();


            var mainConnectionString = Config.GetConnectionString("MainDataStore");
            //mainConnectionString = Encryption.Encrypt(mainConnectionString, AppSettings.ApplicationPassPhrase);

            //mainConnectionString = Encryption.Decrypt(mainConnectionString, AppSettings.ApplicationPassPhrase);

            if (String.IsNullOrWhiteSpace(mainConnectionString))
            {
                throw new ArgumentNullException("MainDataStore connection string property in web.config does not contain a value for connection string");
            }

            Configuration = CreateNewConfigurationUsingConnectionString(mainConnectionString);

            Console.WriteLine("Configuration is null => " + (Configuration == null));

            try
            {
                var result = Configuration.BuildSessionFactory();
                return result;
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                throw;
            }
        }

        private ISessionFactory CreateAuditSessionFactory()
        {
            var connectionString = Config.GetConnectionString("AuditDataStore");

            if (String.IsNullOrWhiteSpace(connectionString))
            {
                return null;
            }

            var configurer = CreatePersistenceConfigurer(connectionString);

            var config = Fluently.Configure()
                .Database(configurer)
                .Mappings(m => m.FluentMappings.AddAuditModels().Conventions.Add<JoinedSubclassIdConvention>());

            var configuration = config.BuildConfiguration();

            var factory = configuration.BuildSessionFactory();

            if (UpdateDatabase)
            {
                new SchemaUpdate(configuration).Execute(false, UpdateDatabase);
            }

            return factory;
        }

        private IPersistenceConfigurer CreatePersistenceConfigurer(string connectionString)
        {
            IPersistenceConfigurer configurer;

            Console.WriteLine("CreatePersistenceConfigurer...1");
            if (connectionString.Contains("##CurrentDirectory##") || connectionString.Contains(":memory:"))
            {
                Console.WriteLine("X.1");
                ProviderName = "SQLITE";
                var currentDirectory = QBicUtils.GetCurrentDirectory();
                Console.WriteLine("Current directory = " + currentDirectory);
                Console.WriteLine("Current directory exists = " + Directory.Exists(currentDirectory));

                var filePath = currentDirectory + "/Data/appData.db";
                Console.WriteLine(filePath + " exists => " + File.Exists(filePath));

                Directory.CreateDirectory(currentDirectory + "/Data");
                Console.WriteLine("Dir " + currentDirectory + "/Data" + " exists = " + Directory.Exists(currentDirectory + "/Data"));


                connectionString = connectionString.Replace("##CurrentDirectory##", currentDirectory); // for my sqlite connectiontion string
                Console.WriteLine("connectionString = " + connectionString);

                configurer = SQLiteConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);
            }
            //else if (providerName.Contains("MySql"))
            //{
            //    ProviderName = "MYSQL";
            //    configurer = MySQLConfiguration.Standard.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);
            //}
            else
            {
                Console.WriteLine("X.2");
                ProviderName = "SQL";
                configurer = MsSqlConfiguration.MsSql2012.ConnectionString(connectionString).IsolationLevel(IsolationLevel.ReadCommitted);
            }
            Console.WriteLine("X.3");

            Console.WriteLine("configurer is null -> " + (configurer == null));

            return configurer;
        }

        public NHibernate.Cfg.Configuration CreateNewConfigurationUsingConnectionString(string connectionString)
        {
            var configurer = CreatePersistenceConfigurer(connectionString);

            var config = Fluently.Configure()
              .Database(configurer)

              .Mappings(m => m.FluentMappings.AddFromRunningAssemblies().Conventions.Add<JoinedSubclassIdConvention>());

            config.ExposeConfiguration(x =>
            {
                x.SetProperty(NHibernate.Cfg.Environment.ShowSql, ShowSql.ToString().ToLower()); // shows sql in console

                // This will set the command_timeout property on factory-level
                //x.SetProperty(NHibernate.Cfg.Environment.CommandTimeout, "180");

                // This will set the command_timeout property on system-level
                //NHibernate.Cfg.Environment.Properties.Add(NHibernate.Cfg.Environment.CommandTimeout, "180");

                x.Properties.Add("use_proxy_validator", "false"); // to ignore public/internal fields on model classes
                                                                  //x.DataBaseIntegration(prop =>
                                                                  //{
                                                                  //    prop.BatchSize = 50;
                                                                  //prop.Batcher<NHibernate.AdoNet.MySqlClientBatchingBatcherFactory>();
                                                                  //});
            });

            var configuration = config.BuildConfiguration();

            return configuration;
        }

        //public void ResetData()
        //{
        //    //new SchemaExport(Configuration).Execute(false, true, false);
        //    new SchemaUpdate(Configuration).Execute(true, true);
        //}

        public ISession OpenAuditSession()
        {
            return AuditStore.OpenSession();
        }

        public IStatelessSession OpenAuditStatelessSession()
        {
            return AuditStore.OpenStatelessSession();
        }

        public ISession OpenSession()
        {
            return Store.OpenSession();
        }

        public IStatelessSession OpenStatelessSession()
        {
            return Store.OpenStatelessSession();
        }
    }
}