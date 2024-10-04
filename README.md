# Machine Management System

This is a **Client-Server Architecture** based **Machine Management System** for monitoring and managing machines across multiple factory locations. The system is built with **C#**, using **TCP/IP** for communication, **ADO.NET** for database access, and **multithreading** to ensure efficient operations. The system supports user-based machine management, with only **admin users** able to add or remove machines.

## Features
- **Real-time Syncing**: Syncs master data from server to client on connection and triggers real-time sync when changes are made.
- **Periodic Transaction Sync**: Transactions (like ping and telnet tests) are synced from client to server every 1 minute.
- **Multithreading**: Client operations (ping/telnet) and server syncing run in separate threads to ensure performance and responsiveness.
- **Base64 Encoding**: Data is transferred between client and server using Base64-encoded DataTables.
- **User Management**: Only users with admin permissions can add or remove machines.
- **Transaction Logging**: Stores the status of ping and telnet tests for machines in the transaction table.


## Technologies Used
- **C#**: Core programming language for client and server.
- **ADO.NET**: Used for database operations.
- **TCP/IP**: Communication protocol between client and server.
- **Multithreading**: For handling concurrent tasks.
- **Base64 Encoding**: To encode DataTables for network transfer.
- **SQL Server**: Backend database for both client and server.

## Database Structure
- **MachineTableMaster**
  - `MachineId` (Primary Key)
  - `MachineName`
  - `IP`
  - `Port`
  - `Image`
  - `Timestamp`
  - `LastUpdated`
- **TransactionTableMaster**
  - `TransactionId` (Primary Key)
  - `M_Id` (Foreign Key to MachineTableMaster)
  - `Event` (Ping/Telnet)
  - `Timestamp`
  - `Status`
- **UserTableMaster**
  - `UserId` (Primary Key)
  - `UserName`
  - `Email`
  - `IsAdmin` (Boolean)

## How to Run

### Prerequisites
- **.NET 6+**
- **SQL Server** (for database)
- **Visual Studio 2022** (or any other IDE with .NET Core support)

### Setup Instructions
1. **Clone the repository**:
   ```bash
   git clone https://github.com/yourusername/MachineManagementSystem.git
   ```
2. **Navigate to the project directory**:
   ```bash
   cd MachineManagementSystem
   ```
3. **Configure Databases**:
   - Update the `connectionString` in both the **Client** and **Server** applications `App.config` to match your SQL Server setup.
   - Ensure the `Service Broker` is enabled for real-time notifications in your SQL Server:
     ```sql
     ALTER DATABASE [YourDatabase] SET ENABLE_BROKER;
     ```
4. **Run the Server**:
   - Open the solution in Visual Studio and set the **Server** as the startup project.
   - Build and run the project.
   - Ensure the **admin users** are set up correctly in the `UserTableMaster`.
5. **Run the Client**:
   - In Visual Studio, set the **Client** as the startup project.
   - Build and run the project. It will start syncing data and performing machine tests (ping/telnet).

## Multithreading Implementation
- **Server**:
  - The server listens for connections in a background thread and syncs data without interrupting the main application thread (where users can add/remove machines).
  
- **Client**:
  - The client performs ping and telnet tests in separate threads, ensuring syncing and test operations do not block the UI or other tasks.

## User Management
- Only users with **admin privileges** (set in the `UserTableMaster`) can add or remove machines. Other users can still interact with the system but have limited permissions.

