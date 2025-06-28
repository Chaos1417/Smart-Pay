SmartPay: Modern Digital Wallet 🚀
🌟 Overview

SmartPay is an intuitive and secure digital wallet application designed to streamline personal financial management. It empowers users to effortlessly manage their balance, track transaction history, add and organize beneficiaries, and conduct secure fund transfers. This project serves as a robust example of a full-stack application, leveraging the power of C# ASP.NET Core for its backend API and a clean, responsive frontend crafted with Vanilla JavaScript, HTML, and Tailwind CSS.
✨ Key Features

    User Authentication & Authorization: Secure user registration and login powered by JWT (JSON Web Token) for stateless authentication.

    Dynamic Dashboard: Provides a real-time overview of current balance, recent transaction activities, and quick access to all core features.

    Seamless Fund Transfers: Facilitates instant fund transfers to approved beneficiaries, complete with customizable transaction types and user-defined descriptions.

    Comprehensive Beneficiary Management: Users can easily add, view, and remove beneficiaries, making recurring transfers quick and efficient.

    Detailed Transaction History: Offers a complete chronological record of all incoming and outgoing financial movements.

    Role-Based Access Control (RBAC): Implements distinct roles for User and Admin, ensuring appropriate access levels (e.g., Admins cannot be added as beneficiaries).

    Robust Security: Implements BCrypt hashing for secure password storage.

    Responsive User Interface: Built with Tailwind CSS to ensure a consistent and optimized experience across desktop, tablet, and mobile devices.

    ACID-Compliant Transactions: Database operations for financial transfers adhere to ACID properties (Atomicity, Consistency, Isolation, Durability) through EF Core transactions.

🛠️ Technologies Used
Backend (ASP.NET Core Web API)

    Language: C#

    Framework: ASP.NET Core 6.0+

    Object-Relational Mapper (ORM): Entity Framework Core

    Database: SQL Server (SQL Server Express / Azure SQL Database / AWS RDS)

    Authentication: JWT (JSON Web Tokens)

    Password Security: BCrypt.Net

    Dependency Management: NuGet

Frontend (Web)

    Languages: HTML5, CSS3, JavaScript (Vanilla JS)

    Styling Framework: Tailwind CSS (via CDN for simplicity)

    Icons: Font Awesome (via CDN)

    API Interaction: Fetch API

🏛️ Architecture Overview

The SmartPay application is built on a standard Client-Server architecture:

    Frontend (Client): A dynamic web interface responsible for user interaction and data presentation. It consumes data from the backend via RESTful API endpoints.

    Backend (Server): An ASP.NET Core Web API that handles all business logic, data processing, user authentication/authorization, and interacts with the SQL Server database.

    Database: A relational SQL Server instance persists all critical application data, including user profiles, transaction records, and beneficiary relationships.

Database Schema (Simplified)

The core database structure includes:

    Users: Stores user credentials, personal details, account balance, and role.

    Beneficiaries: Manages the relationships between an OwnerUser and a BeneficiaryUser.

    Transactions: Logs all financial transfer events between Sender and Receiver users.

🚀 Getting Started

Follow these steps to get your SmartPay application up and running on your local machine.
Prerequisites

Before you begin, ensure you have the following installed:

    .NET SDK 6.0 or higher

    SQL Server instance: This could be SQL Server Express (local), SQL Server Developer Edition, Azure SQL Database, or an AWS RDS SQL Server instance.

    Git

1. Clone the Repository

First, clone the SmartPay repository to your local machine:

git clone https://github.com/Chaos1417/Smart-Pay.git
cd Smart-Pay

2. Backend Setup

Navigate to the Backend directory:

cd Backend

a. Configure Database Connection & JWT Secret

⚠️ Security Warning: Sensitive information like database connection strings and JWT secret keys should NEVER be committed directly to a public GitHub repository.

    appsettings.json: This file serves as a public template. Ensure its sensitive fields are empty or placeholders.

    {
      "ConnectionStrings": {
        "DefaultConnection": "" // Keep this empty for public repo
      },
      "Jwt": {
        "Key": "", // Keep this empty for public repo
        "Issuer": "SmartPay",
        "Audience": "SmartPayUsers"
      },
      "AllowedHosts": "*"
    }

    appsettings.Development.json (Local Secrets): Create this file within your Backend directory if it doesn't exist. This file will contain your actual, sensitive credentials for local development and is ignored by Git (via Backend/.gitignore).

    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=your-aws-rds-endpoint.us-east-1.rds.amazonaws.com;Port=1433;Database=SmartPayDB;User Id=your_db_username;Password=your_db_password;"
      },
      "Jwt": {
        "Key": "YourVeryLongAndComplexJwtSecretKeyThatIsUniqueAndStrongAndAtLeast32CharactersLong", // Replace with a strong, unique key
        "Issuer": "SmartPay",
        "Audience": "SmartPayUsers"
      }
    }

        Replace placeholders:

            your-aws-rds-endpoint...: Your AWS RDS SQL Server endpoint.

            your_db_username: Your RDS master username.

            your_db_password: Your RDS master password.

            YourVeryLongAndComplexJwtSecretKey...: A strong, randomly generated key for JWT signing.

        Ensure AWS RDS Security Group: Make sure your AWS RDS security group allows inbound connections on port 1433 from your current IP address (or 0.0.0.0/0 for testing, but NOT recommended for production).

b. Run Database Migrations

Apply the database schema to your configured SQL Server instance. Make sure dotnet-ef tools are installed (dotnet tool install --global dotnet-ef if needed).

dotnet ef database update

c. Run the Backend Application

Start the backend API server:

dotnet run

The API will typically run on http://localhost:5034 (check your console output for the exact URL).
3. Frontend Setup

Navigate back to the Frontend directory:

cd ../Frontend

a. Verify Backend API URL

Open Js/dashboard.js (and Js/login.js) and ensure the backendBaseUrl constant points to your local backend instance:

// Js/dashboard.js
const backendBaseUrl = 'http://localhost:5034/api'; // This is correct for local testing

b. Open the Application

Simply open the Pages/login.html file in your web browser.

# On Windows
start Pages/login.html
# On macOS
open Pages/login.html
# Or manually navigate to the file path in your browser:
# file:///path/to/Smart-Pay/Frontend/Pages/login.html

