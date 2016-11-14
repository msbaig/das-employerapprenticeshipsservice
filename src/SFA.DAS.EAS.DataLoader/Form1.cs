using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;
using MongoDB.Bson;
using MongoDB.Driver;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.EAS.DataLoader
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void processFile_Click(object sender, EventArgs e)
        {

            IConfigurationRepository configurationRepository = new AzureTableStorageConfigurationRepository(ConfigurationManager.AppSettings["ConfigurationStorageConnectionString"]);
            var configurationService = new ConfigurationService(configurationRepository, new ConfigurationOptions("SFA.DAS.EAS.DataLoader", "LOCAL", "1.0"));
            var configuration = configurationService.Get<DataLoaderConfiguration>();

            var ds = new DataSet();
            var connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + txtExcelFile.Text + ";Extended Properties=\"Excel 12.0;IMEX=1;HDR=NO;TypeGuessRows=0;ImportMixedTypes=Text\"";
            using (var conn = new OleDbConnection(connectionString))
            {
                conn.Open();

                var sheets = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM [" + sheets.Rows[0]["TABLE_NAME"] + "] ";
                    var adapter = new OleDbDataAdapter(cmd);
                    adapter.Fill(ds);
                }
            }
            
            var mongoClient = new MongoClient(configuration.MongoConnectionString);

            var database = mongoClient.GetDatabase("api_test_data");
            var gatewayUsersCollection = database.GetCollection<BsonDocument>("gateway_users");
            var declarationsCollection = database.GetCollection<BsonDocument>("declarations");
            var fractionsCollection = database.GetCollection<BsonDocument>("fractions");

            var table = ds.Tables;
            var declarations = new List<BsonDocument>();
            var fractions = new List<BsonDocument>();
            var submissionId = 0;
            foreach (var row in table[0].Rows)
            {
                var rowValues = row as DataRow;
                if (!string.IsNullOrEmpty(rowValues?.ItemArray[1].ToString()))
                {
                    var levyDueYtd = GetLevyDueYtdValue(rowValues.ItemArray[5].ToString());
                    var fractionDecimal = GetEnglishFractionValue(rowValues.ItemArray[17].ToString());
                    
                    var gatewayUser = GetEmprefFromGatewayUser(rowValues, gatewayUsersCollection);

                    //declarationMongo.DeleteOne(new BsonDocument {{"empref", gatewayUser["empref"].ToString()}});
                    //fractionsMongo.DeleteOne(new BsonDocument {{"empref", gatewayUser["empref"].ToString()}});

                    submissionId++;

                    declarations.Add(CreateDeclarationRecord(gatewayUser, submissionId, levyDueYtd));
                    fractions.Add(CreateEnglishFractionRecord(gatewayUser, fractionDecimal));
                }
            }

            //declarationsCollection.InsertMany(declarations);
            //fractionsCollection.InsertMany(fractions);

            MessageBox.Show("Finished");

        }

        private static BsonDocument GetEmprefFromGatewayUser(DataRow rowValues, IMongoCollection<BsonDocument> gatewayUsers)
        {
            var gateWayLogin = rowValues.ItemArray[1].ToString();
            var filter = Builders<BsonDocument>.Filter.Eq("gatewayID", gateWayLogin);

            var gatewayUser = gatewayUsers.Find(filter).FirstOrDefault();
            return gatewayUser;
        }

        private int GetLevyDueYtdValue(string documentValue)
        {
            var levyDueYtd = 0;
            int.TryParse(documentValue, out levyDueYtd);
            return levyDueYtd;
        }

        private decimal GetEnglishFractionValue(string documentValue)
        {
            var fractionDecimal = 0m;
            decimal.TryParse(documentValue, out fractionDecimal);

            if (fractionDecimal == 0)
            {
                fractionDecimal = 1.00m;
            }
            else
            {
                fractionDecimal = fractionDecimal / 100;
            }

            return fractionDecimal;
        }

        private static BsonDocument CreateEnglishFractionRecord(BsonDocument gatewayUser, decimal fractionDecimal)
        {
            return new BsonDocument{
                //{"_id",new BsonDocument { {"$oid",id} }},
                {"empref",gatewayUser["empref"].ToString() },
                {"fractionCalculations", new BsonArray
                {
                    new BsonDocument
                    {
                        {"calculatedAt", "2016-03-15" },
                        {"fractions", new BsonArray
                        {
                            new BsonDocument
                            {
                                {"region","England"},
                                {"value",fractionDecimal.ToString()}
                            }
                        }
                        }
                    }
                }
                }
            };
        }

        private static BsonDocument CreateDeclarationRecord(BsonDocument gatewayUser, int submissionId, int levyDueYtd)
        {
            return new BsonDocument
            {
                //{"_id",new BsonDocument { {"$oid",id} }},
                {"empref",gatewayUser["empref"].ToString() },
                {"declarations", new BsonArray { new BsonDocument
                {
                    {"id", submissionId },
                    {"submissionTime",DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.000")},
                    {"payrollPeriod",new BsonDocument {
                        {"year","16-17"},
                        {"month",7}
                    } },
                    {"levyDueYTD",levyDueYtd },
                    {"levyAllowanceForFullYear",15000 },
                } }
                }
            };
        }

        private void browseFile_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Filter = "Excel Files|*.xls|All Files|*.*";
                dlg.Multiselect = false;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtExcelFile.Text = dlg.FileName;
                }
            }
        }

        private void txtExcelFile_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
