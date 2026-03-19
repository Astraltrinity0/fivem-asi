# fivem-asi
Updated .ASIs for FiveM, updater app available. [(releases)](https://github.com/tr1nity0/fivem-asi/releases/tag/build). 

## Installation

Self-contained single `.exe`, no `.NET` install required for end users None. (dist `.NET` 8.0 Included)
    
## Supported Builds

```
> Builds 1604 till 3570
1604 | mpchristmas2018 | Arena War | Release
2060 | mpsum | Los Santos Summer Special | Release
2189 | mpheist4 | Cayo Perico Heist | Release
2372 | mptuner | Los Santos Tuners | Release
2545 | mpsecurity | The Contract | Release
2612 | mpg9ec | ExPanDeD aNd EnHanCeD | Release
2699 | mpsum2 | The Criminal Enterprise | Release
2802 | mpchristmas3 | Los Santos Drug War | Release
2944 | mp2023_01 | San Andreas Mercenaries | Release
3095 | mp2023_02 | The Chop Shop | Release
3258 | mp2024_01 | Bottom Dollar Bounties | Release
3407 | mp2024_02 | Agents of Sabotage | Release
3570 | mp2025_01 | Money Fronts | Beta(unstable)
```


## Functionality

- Embeds a `FX_ASI_BUILD` resource under a numeric type ID (e.g. `3258`) with language `1033`
- Uses AsmResolver to read/write the PE resource directory properly
- Build number is embedded as a numeric type ID (`MAKEINTRESOURCE`)
