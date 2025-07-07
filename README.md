SmartPay is an intuitive and secure digital wallet application designed to streamline personal financial management. It enables users to effortlessly manage their balance, track transaction history, add and organize beneficiaries, and conduct secure fund transfers. This project serves as a robust example of a full-stack application, leveraging the power of C# ASP.NET Core for its backend API and a clean, responsive frontend crafted with Vanilla JavaScript, HTML, and Tailwind CSS.

**Key Features**


User Authentication & Authorization: Secure user registration and login powered by JWT (JSON Web Token) for stateless authentication.

Dynamic Dashboard: Provides a real-time overview of current balance, recent transaction activities, and quick access to all core features.

Seamless Fund Transfers: Facilitates instant fund transfers to approved beneficiaries, complete with customizable transaction types and user-defined descriptions.

Comprehensive Beneficiary Management: Users can easily add, view, and remove beneficiaries, making recurring transfers quick and efficient.

Detailed Transaction History: Offers a complete chronological record of all incoming and outgoing financial movements.

Role-Based Access Control (RBAC): Implements distinct roles for User and Admin, ensuring appropriate access levels (e.g., Admins cannot be added as beneficiaries).

Responsive User Interface: Built with Tailwind CSS to ensure a consistent and optimized experience across desktop, tablet, and mobile devices.

ACID-Compliant Transactions: Database operations for financial transfers adhere to ACID properties (Atomicity, Consistency, Isolation, Durability) through EF Core transactions.

**Technologies Used**
Backend (ASP.NET Core Web API)

    Language: C#

    Framework: ASP.NET Core 6.0+

    Object-Relational Mapper (ORM): Entity Framework Core

    Database: SQL Server

    Authentication: JWT (JSON Web Tokens)

Frontend (Web)

    Languages: HTML5, CSS3, JavaScript (Vanilla JS)

    Styling Framework: Tailwind CSS (via CDN for simplicity)

    Icons: Font Awesome (via CDN)

    API Interaction: Fetch API

Database Schema 

The core database structure includes:

    Users: Stores user credentials, personal details, account balance, and role.

    Beneficiaries: Manages the relationships between an OwnerUser and a BeneficiaryUser.

    Transactions: Logs all financial transfer events between Sender and Receiver users.

**Images** 

![Screenshot From 2025-07-07 21-14-18](https://github.com/user-attachments/assets/4dda1c52-8cff-49f6-a67f-93412708d0c0)

![Screenshot From 2025-07-07 21-14-52](https://github.com/user-attachments/assets/7c68d7af-8fdc-4031-a145-78eac5ae8abd)

![Screenshot From 2025-07-07 21-15-01](https://github.com/user-attachments/assets/f625c981-8eb5-498c-b72a-222e79bfe1b2)

![Screenshot From 2025-07-07 21-15-17](https://github.com/user-attachments/assets/60368a9b-409c-4a91-a31c-0900c82c589f)

![Screenshot From 2025-07-07 21-15-24](https://github.com/user-attachments/assets/a7a4742a-5259-4ec5-9b7c-a18baa799c62)

![Screenshot From 2025-07-07 21-15-32](https://github.com/user-attachments/assets/1b9bd781-68cd-4c51-ba06-e098b5182f1e)








