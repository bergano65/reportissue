try
{
   Connect-AzAccount
   Set-AzContext -Subscription $subscription
   $r = Get-AzResource -Name $name -ResourceGroupName $group
   return $r.ResourceId
  
}
catch
{
   return ""
}

