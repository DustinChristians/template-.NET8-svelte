# [.NET 8 Domain Template](https://github.com/DustinChristians/template-.NET8-domain)
A sample .NET 8 CRUD solution to be used as a template for new projects.

# Features
- The [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- A Microsoft SQL Server Local Database with seeded data so you can spin up the project and test it out. See the spin up instructions below.
- Entity Framework Core O/RM with code first migrations.
- A base repository that includes all normal CRUD operations.
- Search methods and extensions for easy searching within repositories.
- Support for bulk database operations like BulkCreate, BulkUpdate and BulkDelete thanks to the free [EFCore.BulkExtensions](https://github.com/borisdj/EFCore.BulkExtensions) package.
- A RESTful API that includes an endpoint for messages, users and project settings. The messages and users tables are used for setting up project patterns and can be removed if they aren't needed.
- An Event Log powered by Serilog. Events are logged to the database and the file system as a backup.
- A Hangfire Scheduler project. There is a single scheduled task that exists to keep the Event Log size down to a minimum. More tasks can easily be added with minimal configuration. Task settings are placed in the appsettings.json file.
- NUnit tests for the existing messages, users and settings repositories. There are also unit tests for the Event Log Cleanup scheduled task.

# [SaaS Starter: A SvelteKit Boilerplate/Template](https://github.com/CriticalMoments/CMSaasStarter)

# Features
- [Feature Rich](#features): user auth, user dashboard, marketing site, blog engine, billing/subscriptions, pricing page, search, emails, and more.
- [Lightning Performance](#performance--best-practices): fast pre-rendered pages which score 100/100 on Google PageSpeed.
- [Delighful Developer Experience](#tech-stack): tools you'll love working with, including SvelteKit, Tailwind, DaisyUI, Postgres, and Supabase.
- Extensible: all the tools you need to make additional marketing pages, UI components, user dashboards, admin portals, database backends, API endpoints, and more.
- [Hosting](#suggested-hosting-stack): Our suggested hosting stack is free to host, cheap to scale, easy to manage, and includes automatic deployments.
- [MIT Open Source](https://github.com/CriticalMoments/CMSaasStarter/blob/main/LICENSE)
- [Fully Functional Demo](https://saasstarter.work)
- [Quick Start](#quick-start): Full docs from `git clone` to deployment.

Created by the folks at [Kiln AI](https://getkiln.ai)! It's the easiest tool for fine-tuning LLM models, synthetic data generation, and collaborating on datasets. The Kiln app was built with SaaS Starter!

**[Kiln AI](https://getkiln.ai)** Rapid AI Prototyping and Dataset Collaboration Tool