<h1 align="center">🗳️ E-Ballot</h1>
<p align="center">
  A secure, lightweight, and modern Online Voting System backend built using ASP.NET Core & Dapper.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/ASP.NET%20Core-Backend-blue?logo=dotnet" />
  <img src="https://img.shields.io/badge/Dapper-MicroORM-green" />
  <img src="https://img.shields.io/badge/JWT-Authentication-orange" />
  <img src="https://img.shields.io/badge/SQL%20Server-Database-red" />
  <img src="https://img.shields.io/badge/License-MIT-lightgrey" />
</p>

---

## 🗳️ What is E-Ballot?

> **E-Ballot** is a secure, role-based online voting backend built for organizations, colleges, and institutions.  
> It ensures **fairness**, **transparency**, and **tamper-proof** election flow using modern authentication and database design.

Built using **Dapper**, focusing on **speed**, **simplicity**, and **clean architecture**.

---

## 🚀 Features

- 🔐 **JWT Authentication** (Admin & Voter roles)
- 🗳️ **Election Management** (Create, Launch, Close)
- 👤 **Candidate Management** per election
- 🗂️ **Voter Management** (Add, view users)
- 🗝️ **One-Vote Security** (A user can vote only once per election)
- 🔒 **AES-Encrypted Votes** (No plaintext stored in DB)
- 📊 **Live Results** using instant SQL aggregation
- ⚡ **Dapper-powered High Performance**
- 🛡️ **Hashed Passwords** with BCrypt


---

## 🛠 Tech Stack

| Layer        | Technology             |
|--------------|------------------------|
| Language     | C# (.NET 9)            |
| Framework    | ASP.NET Core Web API   |
| Database     | SQL Server             |
| ORM          | Dapper (Micro ORM)     |
| Auth         | JWT + BCrypt Hashing    |
| Architecture | 3-Layer Clean Design   |

---

## 📁 Project Structure

