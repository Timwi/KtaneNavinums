@echo off
set /p git_path=""
echo *************************************************************************
echo -----------------------------------------
git init
echo -----------------------------------------
echo *************************************************************************
echo -----------------------------------------
git add .
echo -----------------------------------------
echo *************************************************************************
echo -----------------------------------------
git commit -m "Initial upload"
echo -----------------------------------------
echo *************************************************************************
echo -----------------------------------------
git remote add origin https://github.com/Goofy1807/%git_path%.git
echo -----------------------------------------
echo *************************************************************************
echo -----------------------------------------
git push -u origin master
echo -----------------------------------------
echo *************************************************************************
set /p end="Press enter to continue"