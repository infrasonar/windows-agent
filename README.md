# InfraSonar Windows Agent

Documentation: https://docs.infrasonar.com/collectors/agents/windows/

### Registry

Key: `Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Cesbit\InfraSonarAgent`

Key                     | Type      | Default   | Description
------------------------|-----------| ----------| ------------
ApiUrl                  | REG_SZ    | https://api.infrasonar.com | API Url for InfraSonar
AssetId                 | REG_DWORD | 0 _(initially)_            | Asset Id. When `0`, the agent will create a new asset and change this registry key to the new asset Id.
AssetName               | REG_SZ    | _unset_                    | If set, the asset will be created using this name.
Debug                   | REG_DWORD | _unset_                    | A value of `1` will enable more informational event in the Event Viewer _(Windows Logs/Application)_.
LocalOnly               | REG_DWORD | _unset_                    | A value of `1` will not write to InfraSonar but instead writes output to a tmp file.
Token                   | REG_SZ    | _required_                 | Must be a valid agent token.
CheckInterval/memory    | REG_DWORD | 5                          | Memory check interval in minutes.
CheckInterval/network   | REG_DWORD | 5                          | Memory check interval in minutes.
CheckInterval/process   | REG_DWORD | 5                          | Process check interval in minutes.
CheckInterval/processor | REG_DWORD | 5                          | Processor check interval in minutes.
CheckInterval/services  | REG_DWORD | 5                          | Services check interval in minutes.
CheckInterval/software  | REG_DWORD | 60                         | Software check interval in minutes.
CheckInterval/system    | REG_DWORD | 5                          | System check interval in minutes.
CheckInterval/updates   | REG_DWORD | 5                          | Updates check interval in minutes.
CheckInterval/volume    | REG_DWORD | 5                          | Volume check interval in minutes.

### Development
When updating the agent, do not forget to update the assembly and file version and also update the version of the InfraSonarAgentSetup. 
