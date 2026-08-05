# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0]

- Support `[Until]`-only controllers/methods (no `[From]` required). Previously this threw at startup.
- Honour the `UntilInclusive` option in the controller-level from/until validation, so `[From(x)]`/`[Until(x)]` on the same controller no longer errors when `UntilInclusive` is set.
- Make duplicate-endpoint detection HTTP-verb aware, so the same route with different verbs (e.g. `GET` and `POST`) is no longer flagged as a duplicate.
- Target `net8.0;net9.0;net10.0` (dropped end-of-life `net6.0`/`net7.0`).

## [2.0.1]

- Fix duplicate versions being allowed for major and minor versions

## [2.0.0]

- Included the `DetectDuplicatesAtStartup` option which... will detect, and error out, if a duplicate caused by BetterVersioning.net is found at startup

## [1.0.4]

- Fix GetSupportedVersions not using the UntilInclusive option.

## [1.0.3]

- Nuget shield
- Sonarcloud excludes

## [1.0.1-2]

- Included the README

## [1.0.0]

- Initial release
