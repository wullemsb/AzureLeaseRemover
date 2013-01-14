using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.StorageClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureLeaseRemover
{
    class Program
    {
        static void Main(string[] args)
        {
            var defaultColor = Console.ForegroundColor;

            Console.WriteLine("Please specify a blob URL");

            Uri blobUrl;

            var url = Console.ReadLine();

            blobUrl = new Uri(url,UriKind.Absolute);

            Console.WriteLine("Please specify a storage account name");

            var storageAccountName = Console.ReadLine();

            Console.WriteLine("Please specify a storage account key");

            var storageAccountKey = Console.ReadLine();

            Console.WriteLine("Reading storage account information...");

            var connectionstring = string.Format("DefaultEndpointsProtocol=http;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);

            CloudStorageAccount storageAccount;

            if (!CloudStorageAccount.TryParse(connectionstring, out storageAccount))
            {
                WriteError("The supplied connectionstring does not appear to be a valid storage account");

                return;
            }

            var blobClient = storageAccount.CreateCloudBlobClient();
            blobClient.ServerTimeout = TimeSpan.FromMinutes(1);

            var blob = blobClient.GetBlobReferenceFromServer(blobUrl);


            //TODO: Check whether the blob is currently registered as a disk or image...

            Console.WriteLine("Inspecting the blob's lease status...");
            Console.WriteLine("Current lease status: {0}", blob.Properties.LeaseStatus);

            Console.WriteLine("Unlocking the blob...");
            try
            {
                TimeSpan breakTime = new TimeSpan(0, 0, 1);
                blob.BreakLease(breakTime);
            }
            catch (Exception ex)
            {
                WriteError(string.Format("Error breaking the lease: {0}.", ex.Message));
            }
            
            //Reload blob reference
            blob = blobClient.GetBlobReferenceFromServer(blobUrl);

            Console.WriteLine("Inspecting the blob's lease status...");
            Console.WriteLine("Current lease status: {0}", blob.Properties.LeaseStatus);

            if (blob.Properties.LeaseStatus == LeaseStatus.Unlocked)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Lease removed succesfully.");
            }

            Console.WriteLine("Hit a key to exit...");
            Console.ReadLine();
        }

        private static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.WriteLine("Hit a key to exit...");
            Console.ReadLine();
        }
    }
}
