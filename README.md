# QrPlatform - Event & Invitation Management API

[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive RESTful API built with ASP.NET Core for managing events, attendees, and collaborators, featuring a tiered freemium/pro model and asynchronous invitation generation.

## Project Overview

QrPlatform is a backend system designed to provide a complete, scalable solution for event organizers. It handles the entire event lifecycle, from user authentication and event creation to complex permission management and automated, non-blocking generation of QR-coded invitations. The architecture is built to be robust and maintainable, following modern .NET best practices.

## Key Features

*   **Tiered Event Creation:** Supports both a "Free" tier with basic features and a "Pro" tier with advanced capabilities like custom invitation templates.
*   **Asynchronous Invitation Generation:** Utilizes **Hangfire** to process image and QR code generation in the background, ensuring the API remains fast and responsive.
*   **Role-Based Access Control (RBAC):** A robust collaborator system with distinct roles (Owner, Editor, Check-In Staff) and complex permission rules to manage event access securely.
*   **Secure User Management:** Integrates with **Firebase Authentication** for handling user sign-up and login, with a sync process to keep the application's database up-to-date.
*   **Full Attendee Management:** Complete CRUD (Create, Read, Update, Delete) functionality for managing event attendees, including file-based uploads (.xlsx, .csv).
*   **External Service Integration:** Designed to work with the **Wasender API** for sending WhatsApp invitations and receiving delivery status updates via webhooks.

## Tech Stack

*   **Framework:** ASP.NET Core
*   **Database:** Entity Framework Core with SQL Server
*   **Authentication:** Firebase Authentication (JWT)
*   **Background Jobs:** Hangfire
*   **API Testing:** Postman
*   **Language:** C#

## API Documentation

The complete API specification is available as a PDF within this repository. **Make sure you have added the PDF file to the project's root folder for this link to work.**

**[Click here to view the API Documentation](./QrPlatform%20API%20Specification%202.0.pdf)**
*(Note: I updated the filename to match the one you provided)*

A summary of key endpoints is detailed within the documentation.

## Getting Started

To run this project locally:

1.  **Clone the repository:**
    ```sh
    git clone https://github.com/Null-AH/EventsWebsite.git
    ```
2.  **Configure your secrets:**
    *   This project uses the .NET Secret Manager. Initialize it by running `dotnet user-secrets init`.
    *   Set your secrets for the database connection string (`ConnectionStrings:DefaultConnection`) and other API keys using the `dotnet user-secrets set` command.
3.  **Apply database migrations:**
    ```sh
    dotnet ef database update
    ```
4.  **Run the application:**
    ```sh
    dotnet run
    ```