using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using System;
using System.Security.Cryptography;

namespace functionapp01_TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string azstorageaccnt01_connection = "DefaultEndpointsProtocol=https;AccountName=azstorageaccnt01;AccountKey=s9FysFde5b7D5GbrCWsgyYqLNNxw65xvFqdler10aibcvLC8sL2a0On96wQ/j08gxNSs65mBHpKAQ6nMB/CG6g==;EndpointSuffix=core.windows.net";

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(azstorageaccnt01_connection);
            CloudQueueClient cloudQueueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue cloudQueue = cloudQueueClient.GetQueueReference("az-strorage-queue01"); //name of storage queue

            for (int i = 0; i < 20000; i++)
            {
                CloudQueueMessage queueMessage = new CloudQueueMessage("New Data: " + System.DateTime.Now.ToString()); //message to write in the queue

                cloudQueue.AddMessage(queueMessage);
                Console.WriteLine("Message:" + i.ToString());

            }

        
        }
    }
}
