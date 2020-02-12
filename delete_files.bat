git filter-branch --tree-filter "rm -rf OPU\ApaxLib\ModBusSerialMaster.cs" HEAD
git filter-branch --tree-filter "rm -rf OPU\ApaxLib\FuncBool.cs" HEAD
git filter-branch --tree-filter "rm -rf OPU\ApaxLib\Function.cs" HEAD
git push origin master --force