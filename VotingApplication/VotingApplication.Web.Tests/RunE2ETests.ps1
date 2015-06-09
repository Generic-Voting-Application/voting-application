# Wake the site up
Invoke-WebRequest -Uri "http://localhost:64205/" -TimeoutSec 360

# Replace the db connection with the local instance
$startPath = "VotingApplication\VotingApplication.Web.Tests\bin\Appveyor\"
$config = join-path $startPath "VotingApplication.Web.Tests.dll.config"
$doc = (gc $config) -as [xml]
$doc.SelectSingleNode('//connectionStrings/add[@name="TestVotingContext"]').connectionString = 'Server=(local)\SQL2012SP1;User ID=sa;Password=Password12!'
$doc.Save($config)


#Run the E2E tests
$testpath = "VotingApplication.Web.Tests\bin\Appveyor\VotingApplication.Web.Tests.dll"

vstest.console /logger:Appveyor $testpath /TestCaseFilter:"TestCategory=E2E"