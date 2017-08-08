# Up in Smoke - Wellington DevOps Meetup

Lightning talk to the Wellington DevOps Meetup: 8/8/17.

See `Smoke.Tests.ps1` for the tests used in the demo.

## Intro

Pester is an opensource BDD framework for PowerShell. It's primary use case is _unit
testing PowerShell scripts_, but I find it really useful for _integration tests_, 
_smoke tests_, _database tests_ and _API tests_.

> <https://github.com/pester/Pester>

## ISE

VSCode is the new PowerShell ISE. It has built-in terminal, debugging with breakpoints
and more.

> <https://code.visualstudio.com/>

## New-Fixture

    New-Fixture -Path Tests -Name Smoke

## Do something awesome

```powershell
function DoSomethingAwesome{
    "Did it"
}

Describe "DoSomethingAwesome" {
    It "does something awesome" {
        DoSomethingAwesome | Should Be "Did it"
    }
}

# Pass. Does something awesome
```

## Mocking

```powershell
function DoSomethingAwesome{
    "Did it"
}

Describe "DoSomethingAwesome" {
    Mock DoSomethingAwesome { return "Not awesome" }

    It "does something awesome" {
        DoSomethingAwesome | Should Be "Did it"
    }
}

# Fail: Expected: {Did it} But was:  {Don't done did it}
```

## Smoke test

```powershell
Describe "/health endpoint" {
    $result = Invoke-RestMethod -Method Get -Uri https://putauaki-api.azurewebsites.net/api/health

    It "Returns Status OK" {
        $result.Status | Should Be "Ok"
    }

    It "should be version 1.0.1" {
        $result.Version | Should Be "1.0.1"
    }
}
```

## Invoke-Pester

Invoke Pester from the command line or from your favourite CI/CD server. Generates
NUnit Test Report XML.

     Invoke-Pester -OutputFormat NUnitXml -OutputFile TestOutput.xml

## Examples

<https://github.com/DanielLarsenNZ/examples>
