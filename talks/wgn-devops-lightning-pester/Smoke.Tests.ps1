# Function under test
function DoSomethingAwesome {
    "Something Awesome"
}

# Does something awesome
Describe "Do something awesome"{
    It "Does something awesome"{
        DoSomethingAwesome | Should Be "Something awesome"
    }
}

# Mock the DoSomethingAwesome function
Describe "Do something awesome"{
    Mock DoSomethingAwesome { return "Not awesome"}

    It "Does something awesome"{
        DoSomethingAwesome | Should Be "Something awesome"
    }
}

# Smoke test the Health API - Status and Version number.
Describe "/health API"{
    $result = Invoke-RestMethod -Method Get -Uri https://putauaki-api.azurewebsites.net/api/health
    
    It "Returns Status OK"{
        $result.Status | Should Be "Ok"
    }

    It "Should be Version 1.0.1" {
        $result.Version | Should Be "1.0.1"
    }
}
