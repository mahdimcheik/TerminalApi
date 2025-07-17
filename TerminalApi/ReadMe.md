# Test Run and report results
## Run Tests:
Dans le projet TerminalTest/TestIntegration, vous pouvez exécuter les tests unitaires en utilisant la commande suivante dans le terminal :
```bash
dotnet test --logger "trx;LogFileName=results.trx"
```
## Report Results:
Dans le projet TerminalTest/TestIntegration, vous pouvez générer un rapport de test en utilisant la commande suivante dans le terminal :
### pour installer le report generator global tool :
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

```bash
reportgenerator -reports:./TestResults/results.trx -targetdir:Report -reporttypes:Html
```
et ensuite , ouvrez le fichier `index.html` dans le dossier `Report` pour visualiser le rapport de test.