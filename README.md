# QrPlatform - Event & Invitation Management API

[![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)](https://dotnet.microsoft.com/)

A comprehensive RESTful API built with ASP.NET Core for managing events, attendees, and collaborators, featuring a tiered freemium/pro model and asynchronous invitation generation.

## Project Overview

QrPlatform is a backend system designed to provide a complete, scalable solution for event organizers. It handles the entire event lifecycle, from user authentication and event creation to complex permission management and automated, non-blocking generation of QR-coded invitations. The architecture is built to be robust and maintainable, following modern .NET best practices.

## Key Features

*   **Secure User Authentication & Sync:**  Integrates with **Firebase Authentication** for handling user sign-up and login, with a sync process to keep the application's database up-to-date.

*   **Event & Attendee Management:** Full CRUD (Create, Read, Update, Delete) functionality for events and attendees. Supports bulk attendee create, edit and delete, with robust server-side validation.

*   **Role-Based Access Control (RBAC):** Features a robust, role-based permission system with distinct roles (**Owner**, **Editor**, **Check-In Staff**). Implements complex business logic for creator privileges and rules to ensure event ownership is never lost, demonstrating a deep understanding of authorization patterns.

*   **Asynchronous & Customizable Invitation Generation:** Leverages **Hangfire** for non-blocking background processing to generate personalized invitation images with embedded QR codes. The system is designed for customization, supporting different background images and a variety of **custom fonts** to match event branding.

*   **Automated WhatsApp Invitations & Status Tracking:** Integrates with the **Wasender API** to send generated invitations directly to attendees via WhatsApp. Includes a dedicated **webhook endpoint** to receive real-time delivery status updates (`Sent`, `Delivered`, `Read`), providing valuable feedback to the event organizer.

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

## Connect With Me

If you have any questions about this project or would like to connect, you can find me here:

*   **LinkedIn:** [https://www.linkedin.com/in/ahmed-almazni](https://www.linkedin.com/in/ahmed-almazni)
*   **GitHub:** [https://github.com/Null-AH](https://github.com/Null-AH)