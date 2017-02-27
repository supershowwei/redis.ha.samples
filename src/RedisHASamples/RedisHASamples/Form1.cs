using System;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using StackExchange.Redis;

namespace RedisHASamples
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            this.InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = $"Master EndPoint: {this.GetCurrentMasterEndPoint()}";
        }

        private EndPoint GetCurrentMasterEndPoint()
        {
            var sentinelConfig = new ConfigurationOptions
                                     {
                                         ServiceName = "mymaster",
                                         TieBreaker = string.Empty,
                                         CommandMap = CommandMap.Sentinel,
                                         DefaultVersion = new Version(3, 0),
                                         AllowAdmin = true
                                     };
            sentinelConfig.EndPoints.Add("192.168.8.131", 26379);

            var sentinelConnection = ConnectionMultiplexer.Connect(sentinelConfig);
            var endPoint = sentinelConnection.GetEndPoints().First();
            var server = sentinelConnection.GetServer(endPoint);

            return server.SentinelGetMasterAddressByName("mymaster");
        }
    }
}