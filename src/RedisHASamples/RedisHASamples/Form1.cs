using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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

        private void button2_Click(object sender, EventArgs e)
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
            var sentinelSubscriber = sentinelConnection.GetSubscriber();
            sentinelSubscriber.Subscribe(
                "+switch-master",
                (channel, value) =>
                    {
                        var newMasterEndPoint = this.GetCurrentMasterEndPoint(value);

                        this.textBox1.InvokeIfNecessary(
                            () => { this.textBox1.Text = $"Subscribed Master EndPoint: {newMasterEndPoint}"; });
                    });
        }

        private EndPoint GetCurrentMasterEndPoint(RedisValue value)
        {
            var regex = Regex.Match(
                value.ToString(),
                @"(?<masterName>.*)\s(?<fromHost>.*)\s(?<fromPort>.*)\s(?<toHost>.*)\s(?<toPort>.*)");

            return new IPEndPoint(
                       IPAddress.Parse(regex.Groups["toHost"].Value),
                       int.Parse(regex.Groups["toPort"].Value));
        }
    }
}