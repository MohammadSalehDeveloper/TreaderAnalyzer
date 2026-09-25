# Web layer

The public site lives in `src/Web/Web.Client`, a Blazor WebAssembly client. It renders pages and calls existing account APIs. It does not own domain rules or persistence.

`Web.App` and `Web.Components` are still the original scaffolds.

## Public shell

`Layout/MainLayout.razor` is the default layout: `SiteHeader`, the page body, then `SiteFooter`.

| Piece | File | Role |
|-------|------|------|
| Header | `Layout/SiteHeader.razor` | Logo, Trading, Platforms, Hubs, About Us, search, download, Login |
| Section links | `Layout/SectionLink.razor` | Scrolls to an element id on the home page |
| Footer | `Layout/SiteFooter.razor` | Explore links, Hubs, About Us, account links |
| Home | `Pages/Home.razor` | Hero animation, trading panels and AI system, platforms |

Login, register, forgot password, and reset password keep `@layout AuthLayout`, so they do not show this shell.

Header targets:

| Control | Destination |
|---------|-------------|
| Trading | `#trading` |
| Platforms | `#platforms` |
| Hubs | `#hubs` in the footer |
| About Us | `#about` in the footer |
| Search | Filters those sections plus Login |
| Download | `#download` on the desktop platform card |
| Login | `/login` |

The hero chart reuses `wwwroot/js/login-chart.js`. It is a sample animation, not a live price feed. The desktop card does not download a file; the installer is not published yet.

## What not to do

- Do not put trading rules, validation, or EF in the web project.
- Do not add a UI package without approval.
- Do not point Download at a fake installer.
