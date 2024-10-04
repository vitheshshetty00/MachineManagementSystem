// See https://aka.ms/new-console-template for more information
using Clinet.DbAccess;
using Client.Network;

DbCreation.InitializeDB();
Console.WriteLine("DB Initialized");
Task clientHandle =Task.Run(() => new ClientNetworkManager().Start());

clientHandle.Wait();

