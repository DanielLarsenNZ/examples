$location = 'australiaeast'
$rg = 'pesterarm-rg'
$tags = 'project=Pester ARM tests'
$vm = "pesterarm-1"

# Deploy resource group and VM
az group create -n $rg --location $location --tags $tags
az vm create -n $vm -g $rg --image UbuntuLTS --tags $tags --public-ip-address '""'

# Tests on deployed state

Describe "IP Addresses" {
    $ipsDeployed = ( az vm list-ip-addresses -g $rg -n $vm | ConvertFrom-Json )

    It "has private ip" {
        $ipsDeployed.virtualMachine.network.privateIpAddresses.Length | Should -Be 1
    }

    It "has no public ip" {
        $ipsDeployed.virtualMachine.network.publicIpAddresses.Length | Should -Be 0
    }    
}

Describe "VM" {
    $vmDeployed = ( az vm show -n $vm -g $rg | ConvertFrom-Json )

    It "has OS disk space at least 30GB" {
        $vmDeployed.storageProfile.osDisk.diskSizeGb | Should -BeGreaterOrEqual 30
    }

    # etc...
}


# Tear down
# az group delete -n $rg --yes