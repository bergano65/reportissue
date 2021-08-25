try
{
        $subscription = "bcbd775a-813c-46e8-afe5-1a66912e0f03"
       $group = "evitenres"
        $name = "alert-tst"

   #$name = "20047d37-a1d7-466b-9e2a-ece6fe5b4d0a"
   #$pwd = "14mqem2ISQ0.E.E6-xV6p-YtHluR-FDn4R"
           
   #$pscredential = New-Object -TypeName System.Management.Automation.PSCredential($name,( ConvertTo-SecureString $pwd))
   #Connect-AzAccount -Credential $pscredential
   Set-AzContext -Subscription $subscription
   $r = Get-AzResource -Name $name -ResourceGroupName $group
   return $r.ResourceId
  }
  catch
{
   return "error"
}



