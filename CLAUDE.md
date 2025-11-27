# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Umbraco Commerce Checkout** is an add-on package for Umbraco Commerce that provides a ready-made, themeable, and responsive checkout flow. It is built as an ASP.NET Razor SDK project targeting .NET 8+ and Umbraco 16+.

This package follows the same versioning strategy as Umbraco Commerce and is part of the larger Umbraco Commerce ecosystem.

## Building the Project

### Prerequisites
- .NET 10.0 SDK (see `global.json` for exact version)
- Node.js 20+ for frontend development
- Umbraco 16.0.0 or newer
- Umbraco Commerce 17.0.0 or newer

### Build Commands

**Build the entire solution:**
```bash
dotnet build Umbraco.Commerce.Checkout.sln --configuration Release
```

**Restore NuGet packages (locked mode):**
```bash
dotnet restore Umbraco.Commerce.Checkout.sln --locked-mode
```

**Create NuGet package:**
```bash
dotnet pack Umbraco.Commerce.Checkout.sln --configuration Release --output ./artifacts
```

### Frontend Development

The frontend code is located in `src/Umbraco.Commerce.Checkout/Client/` and uses Vite for bundling.

**Install frontend dependencies:**
```bash
cd src/Umbraco.Commerce.Checkout/Client
npm ci
```

**Build frontend (one-time):**
```bash
cd src/Umbraco.Commerce.Checkout/Client
npm run build
```

**Watch mode for development:**
```bash
cd src/Umbraco.Commerce.Checkout/Client
npm run watch
```

**Lint frontend code:**
```bash
cd src/Umbraco.Commerce.Checkout/Client
npm run lint
```

## Architecture

### Backend Architecture

The package is structured as an ASP.NET Razor SDK library with static web assets deployed to `App_Plugins/UmbracoCommerceCheckout`.

**Key Components:**

1. **Composing** (`Composing/`)
   - `UmbracoCommerceCheckoutComposer`: Auto-registers the package with Umbraco's DI container
   - Composes after `UmbracoCommerceComposer` to ensure proper initialization order

2. **Configuration** (`Configuration/`)
   - `UmbracoCommerceCheckoutSettings`: Configurable settings bound from `appsettings.json` under `Umbraco:Commerce:Checkout`

3. **Extensions** (`Extensions/`)
   - Extension methods for Umbraco and Commerce objects
   - `CompositionExtensions`: DI registration helpers
   - `CustomerExtensions`: Customer data manipulation
   - `PublishedContentExtensions`: Umbraco content helpers

4. **Pipeline** (`Pipeline/`)
   - Install pipeline for setting up checkout nodes in Umbraco
   - `InstallPipeline`: Orchestrates installation tasks
   - `Tasks/`: Individual installation steps (create data types, document types, nodes, payment methods, configure stores)

5. **Web Layer** (`Web/`)
   - **Controllers**: MVC/Surface controllers for checkout flow
     - `UmbracoCommerceCheckoutBaseController`: Base class with shared checkout logic
     - `UmbracoCommerceCheckoutCheckoutPageController`: Renders checkout pages
     - `UmbracoCommerceCheckoutCheckoutStepPageController`: Handles individual step pages
     - `UmbracoCommerceCheckoutSurfaceController`: Form submission endpoints
     - `UmbracoCommerceCheckoutApiController`: API endpoints for the checkout flow
   - **DTOs**: Data transfer objects for API payloads
   - **Filters**: Action filters like `NoStoreCacheControlAttribute`

6. **Views** (`Views/UmbracoCommerceCheckout/`)
   - Razor views for each checkout step
   - Layout: `UmbracoCommerceCheckoutLayout.cshtml`
   - Step pages: Information, Shipping Method, Payment Method, Review, Confirmation
   - Partials: Reusable view components

7. **Events** (`Events/`)
   - Notification handlers for Umbraco and Commerce events
   - `SetStoreCheckoutRelation`: Links stores to checkout pages
   - `SyncZeroValuePaymentProviderContinueUrl`: Updates zero-value payment continue URLs
   - `ResetPaymentAndShippingMethods`: Clears cached methods on cache refresh

8. **Services** (`Services/`)
   - `InstallService`: Orchestrates the installation process

### Frontend Architecture

The frontend is split into two entry points built with Vite + TypeScript:

1. **Backoffice** (`Client/src/backoffice/`)
   - Built as `uccheckout.backoffice.js`
   - Umbraco backoffice integration (dashboards, modals)
   - Installation API and UI for setting up checkout pages
   - Uses Umbraco's Lit-based web components

2. **Surface** (`Client/src/surface/`)
   - Built as `uccheckout.surface.js` + `uccheckout.surface.css`
   - Frontend checkout UI (customer-facing)
   - Styled with Tailwind CSS
   - Provides interactive checkout experience

**Build Output:**
- Frontend builds to `src/Umbraco.Commerce.Checkout/wwwroot/`
- Static assets are packaged as Razor SDK static web assets
- Assets are deployed to `App_Plugins/UmbracoCommerceCheckout` at runtime

### Key Patterns

**Dependency Injection:**
- Uses `AddUmbracoCommerceCheckout()` extension method for registration
- Registers as singleton services where appropriate
- Notification handlers registered via `AddNotificationAsyncHandler<,>()`

**Store-Checkout Relationship:**
- `StoreCheckoutRelationHelper` manages relationships between Commerce stores and checkout page nodes
- Content cache refresher notifications keep relationships synchronized

**Zero-Value Payment:**
- Package creates a special "Zero Value Payment" payment method during installation
- Used for orders with 0 total (e.g., 100% discount code applied)
- Continue URL synchronized when checkout pages are published/unpublished

**Install Pipeline:**
- Extensible pipeline pattern for installation tasks
- Tasks run in sequence: Data Types → Document Types → Nodes → Payment Method → Store Configuration
- Each task is a separate class implementing `IAsyncPipelineTask<InstallPipelineData>`

## Project Structure

```
Umbraco.Commerce.Checkout.vLatest/
├── src/
│   └── Umbraco.Commerce.Checkout/          # Main package project
│       ├── Client/                          # Frontend code (TypeScript/Vite)
│       │   ├── src/
│       │   │   ├── backoffice/             # Backoffice UI
│       │   │   └── surface/                # Frontend checkout UI
│       │   ├── package.json
│       │   ├── vite.config.js
│       │   └── tailwind.config.js
│       ├── Composing/                       # DI composition
│       ├── Configuration/                   # Settings
│       ├── Events/                          # Event handlers
│       ├── Extensions/                      # Extension methods
│       ├── Helpers/                         # Helper classes
│       ├── Models/                          # Domain models
│       ├── Pipeline/                        # Install pipeline
│       │   └── Tasks/                       # Install tasks
│       ├── Services/                        # Application services
│       ├── Views/
│       │   └── UmbracoCommerceCheckout/    # Razor views
│       ├── Web/                             # Web layer
│       │   ├── Controllers/                 # MVC controllers
│       │   └── Dtos/                        # API DTOs
│       └── wwwroot/                         # Static assets (built from Client/)
├── demos/
│   └── v16-blendid/                        # Demo store project
│       ├── Umbraco.Commerce.DemoStore/     # Demo store library
│       └── Umbraco.Commerce.DemoStore.Web/ # Demo web project
├── Directory.Build.props                    # Shared MSBuild properties
├── Directory.Packages.props                 # Central package management
├── version.json                             # GitVersioning configuration
└── azure-pipelines.yml                      # CI/CD pipeline
```

## Version Management

- **Version File**: `version.json` (Nerdbank.GitVersioning)
- **Version Pattern**: Matches Umbraco Commerce major version
- **Package Dependencies**: Defined in `Directory.Packages.props` with version ranges

**Important:** When updating version numbers, ensure:
1. Update `version.json` (NerdBank.GitVersioning controls assembly/NuGet versions)
2. Update `Client/package.json` to match
3. Dependency ranges in `Directory.Packages.props` reference the correct Umbraco Commerce version range

## Common Development Workflows

### Making Changes to Frontend Code

1. Navigate to Client directory: `cd src/Umbraco.Commerce.Checkout/Client`
2. Ensure dependencies are installed: `npm ci`
3. Run watch mode: `npm run watch`
4. Make changes to TypeScript files in `src/backoffice/` or `src/surface/`
5. Vite will rebuild automatically to `wwwroot/`

### Making Changes to Backend Code

1. Modify C# files in appropriate directories
2. Build solution: `dotnet build Umbraco.Commerce.Checkout.sln`
3. If views changed, they are copied automatically during build
4. Test using the demo store

### Adding New Install Pipeline Tasks

1. Create new task class in `Pipeline/Tasks/`
2. Implement `IAsyncPipelineTask<InstallPipelineData>`
3. Register in `Extensions/CompositionExtensions.cs` via `AddUmbracoCommerceInstallPipeline()`
4. Task will execute during package installation

## Important Constraints

- **Backwards Compatibility**: All development should prioritize backwards compatibility to minimize breaking changes
- **Target Frameworks**: Multi-target .NET 8, 9, and 10 (check `.csproj` for current targets)
- **Umbraco Version**: Targets Umbraco 17.x (beta currently)
- **Commerce Version**: Requires Umbraco Commerce 17.x

## CI/CD Pipeline

The Azure DevOps pipeline (`azure-pipelines.yml`) triggers on:
- Branch pushes: `dev`, `release/*`, `hotfix/*`, `support/*`, `feature/*`
- Tags: `release-*`

**Pipeline Steps:**
1. Restore NPM packages (with caching)
2. Build frontend (`npm run build`)
3. Restore NuGet packages (with optional caching)
4. Build solution (`dotnet build`)
5. Pack solution (`dotnet pack`)
6. Publish artifacts (nupkg and build output)
