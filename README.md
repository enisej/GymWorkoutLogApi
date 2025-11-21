# GymWorkoutLogApi

**GymWorkoutLogApi** is an ASP.NET Core Web API project for tracking workouts, exercises, and muscle group statistics. It provides endpoints for logging workouts, querying exercises, and generating statistics with optional user filters.

---

## Features

- CRUD operations for **Body Parts** and **Exercises**
- Log workouts and sets with weight, reps, and notes
- Query statistics:
  - Top 10 heaviest sets
  - Sets above a configurable weight
  - Sessions by specific date
  - Exercises by muscle group
  - Sessions in the last 30 days
  - Most popular muscle groups
- JSON output formatted with indents and null values ignored
- Database migrations and seed data included
- CORS configured for integration with a Vue.js frontend

---

## Technology Stack

- **.NET 9.0** (ASP.NET Core Web API)
- **Entity Framework Core 9.0** with **PostgreSQL**
- **Swashbuckle / Swagger** for API documentation
---

