# Testing this thing by hand

The automated tests cover the logic, but sometimes you just want to click around and see the
versioned docs render for real. Here's how I do it when I want to sanity-check a change before
pushing.

There are three example apps, and between them they show off the two supported UIs over both
document generators:

- **`examples/Swashbuckle`** – the classic Swagger UI, driven by Swashbuckle's own generator.
- **`examples/OpenApiScalar`** – the newer Microsoft OpenAPI pipeline rendered with Scalar.
- **`examples/OpenApiSwaggerUi`** – the Microsoft OpenAPI pipeline, but rendered with Swagger UI
  (proof the documents are UI-agnostic: same generator as the Scalar example, different UI).

All three share the same set of controllers and the same list of versions, so if the versioning
convention is doing its job you should see identical behaviour across all of them, just wrapped
in a different UI.

## The fast way: F5 in VS Code

If you're in VS Code, this is the least amount of effort. Open the Run and Debug panel, pick one
of these from the dropdown, and hit play:

- **Swashbuckle example (Swagger UI)**
- **OpenAPI + Scalar example**
- **OpenAPI + Swagger UI example**

Each one builds the example first, boots it on a fixed port, and pops open your browser straight
at the right page (Swagger at `:5001/swagger`, Scalar at `:5002/scalar`, Swagger-over-OpenAPI at
`:5003/swagger`). Set a breakpoint in a controller or in the convention builder if you want to
watch it work.

Want all three at once? Pick the **All examples (Swagger UI + Scalar + Swagger-over-OpenAPI)**
compound profile, it launches every example together on its own port and opens all three UIs, so
you can eyeball the same version behaviour in each side by side.

## The terminal way

Nothing wrong with a plain `dotnet run`. Ports are pinned here so the curl commands below line up:

```bash
# Swagger example
dotnet run --project examples/Swashbuckle/Swashbuckle.Example.csproj --urls http://localhost:5001

# Scalar example (run in a second terminal, different port)
dotnet run --project examples/OpenApiScalar/OpenApiScalar.Example.csproj --urls http://localhost:5002

# Swagger UI over the Microsoft OpenAPI documents (third terminal)
dotnet run --project examples/OpenApiSwaggerUi/OpenApiSwaggerUi.Example.csproj --urls http://localhost:5003
```

Then open:

- Swagger UI → <http://localhost:5001/swagger>
- Scalar UI → <http://localhost:5002/scalar>
- Swagger UI over Microsoft OpenAPI → <http://localhost:5003/swagger>

In both, there's a document/version picker. You should see every version listed: `v1`, `v2`,
`v6`, `v31`, `v32`, `v33`, `v34`, `v37`.

## What's actually worth looking at

The whole point of this library is that a single set of controllers gets sliced into the right
shape per version. So don't just check that the page loads, check that the *right endpoints show
up in the right versions*. A few things I always eyeball:

- **`Legacy` disappears after v31.** `LegacyController` is marked `[Until(31)]`, so it should be
  present in `v1` through `v31` and gone from `v32` onward.
- **`BetterVersions` shows up from v6.** The controller is `[From(6)]`, so it's absent in `v1`/`v2`
  and present from `v6`.
- **The minor-version endpoints come and go.** For example `new-only-minors` is
  `[From(34,1)]`/`[Until(34,2)]`, so it only exists in the `v34` document, not `v37`.
