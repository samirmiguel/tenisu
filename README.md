# Tenisu API

API REST prête pour la production, construite avec **.NET 9 / ASP.NET Core**, qui expose des données et statistiques sur des joueurs de tennis en suivant les principes de la **Clean Architecture**.

---

## Architecture

```
Tenisu.sln
├── src/
│   ├── Tenisu.Domain/          # Entités, exceptions métier — aucune dépendance externe
│   ├── Tenisu.Application/     # DTOs, interfaces, logique métier, validation
│   ├── Tenisu.Infrastructure/  # Dépôt JSON (store en mémoire)
│   └── Tenisu.API/             # Contrôleurs, middleware, injection de dépendances, Program.cs
└── tests/
    └── Tenisu.Tests/           # Tests unitaires xUnit + Moq

Flux de dépendances :
  API → Application → Domain
  API → Infrastructure → Application → Domain
```

---

## Prérequis

| Outil | Version minimale |
|-------|-----------------|
| .NET SDK | 9.0+ |
| Docker | 24+ |
| Azure CLI | 2.60+ |

---

## Lancer en local

```bash
dotnet run --project src/Tenisu.API
```

L'API démarre sur `https://localhost:5001` / `http://localhost:5000`.  
L'interface Swagger est disponible sur `http://localhost:5000/swagger` (environnement développement) et sur `https://tenisu-api.blackbay-ed1c0316.northeurope.azurecontainerapps.io/swagger/index.html` (environnement de production)

---

## Lancer avec Docker

```bash
# Construction de l'image
docker build -t tenisu-api .

# Démarrage du conteneur
docker run -p 8080:8080 tenisu-api
```

L'API est accessible sur `http://localhost:8080`.

---

## Lancer les tests

```bash
dotnet test
```

Les 13 tests unitaires doivent passer (7 `PlayerServiceTests` + 4 `StatsServiceTests` + 2 cas limites).

---

## Référence API

| Méthode | Route | Description | Code de succès |
|---------|-------|-------------|----------------|
| `GET` | `/api/players` | Liste de tous les joueurs triés par rang (meilleur en premier). Filtre optionnel `?sex=M\|F`. | `200` |
| `GET` | `/api/players/{id}` | Un joueur par identifiant. | `200` / `404` |
| `GET` | `/api/players/stats` | Statistiques agrégées (meilleur pays, IMC moyen, taille médiane). | `200` |
| `POST` | `/api/players` | Ajoute un joueur dans le store en mémoire. Retourne un en-tête `Location`. | `201` / `400` / `409` |
| `DELETE` | `/api/players/{id}` | Supprime un joueur du store en mémoire. | `204` / `404` |
| `GET` | `/health` | Vérification de santé — s'assure que le fichier de données est lisible. | `200` |

### Exemple : GET /api/players/stats

```json
{
  "bestCountry": "SRB",
  "averageBmi": 23.36,
  "medianHeight": 185
}
```

### Exemple : POST /api/players

```json
{
  "id": 200,
  "firstname": "Carlos",
  "lastname": "Alcaraz",
  "shortname": "C.ALC",
  "sex": "M",
  "country": { "code": "ESP", "picture": "" },
  "picture": "",
  "data": {
    "rank": 1,
    "points": 9000,
    "weight": 79000,
    "height": 185,
    "age": 21,
    "last": [1, 1, 1, 1, 0]
  }
}
```

---

## Déploiement sur Azure

Azure CLI et Docker doivent être installés en local.

```bash
chmod +x azure-deploy.sh
./azure-deploy.sh
```

Le script effectue les opérations suivantes :
1. Connexion via `az login`
2. Enregistrement des providers Azure nécessaires
3. Création du groupe de ressources `rg-tenisu`
4. Création d'un Azure Container Registry `tenisuacr`
5. Construction et publication de l'image Docker en local puis push vers l'ACR
6. Création d'un environnement Container Apps et déploiement sur le port 8080
7. Affichage de l'URL publique

### URL de production

| Route | URL |
|-------|-----|
| Joueurs | https://tenisu-api.blackbay-ed1c0316.northeurope.azurecontainerapps.io/api/players |
| Filtre par sexe | https://tenisu-api.blackbay-ed1c0316.northeurope.azurecontainerapps.io/api/players?sex=M |
| Joueur par ID | https://tenisu-api.blackbay-ed1c0316.northeurope.azurecontainerapps.io/api/players/17 |
| Statistiques | https://tenisu-api.blackbay-ed1c0316.northeurope.azurecontainerapps.io/api/players/stats |
| Swagger UI | https://tenisu-api.blackbay-ed1c0316.northeurope.azurecontainerapps.io/swagger |
| Health | https://tenisu-api.blackbay-ed1c0316.northeurope.azurecontainerapps.io/health |

---

## Notes sur le modèle de données

- `data.weight` est stocké en **grammes** (diviser par 1000 pour obtenir les kg)
- `data.height` est en **centimètres**
- `data.last` est un tableau de 5 résultats de matchs au maximum : `1` = victoire, `0` = défaite
- Les données sont chargées une seule fois au démarrage depuis `src/Tenisu.Infrastructure/Data/players.json` ; les mutations (POST / DELETE) sont **en mémoire uniquement** et ne survivent pas à un redémarrage

---

## Limitations connues et améliorations possibles

- **Pas de persistance** — le store en mémoire est réinitialisé à chaque redémarrage. Une version future pourrait s'appuyer sur une base de données (PostgreSQL via EF Core).
- **Pas d'authentification** — les endpoints POST et DELETE sont ouverts. L'ajout d'une authentification JWT ou par clé API serait la première étape de sécurisation.
- **Instance unique** — le store en mémoire n'est pas partagé entre les réplicas. Un passage à l'échelle horizontale nécessite un store externe.
- **Invalidation totale du cache** — à chaque écriture, toutes les entrées en cache sont supprimées. Suffisant à cette échelle ; un cache taggé serait plus chirurgical.
- **Pas de pagination** — la liste de joueurs est courte, mais `GET /api/players` devrait accepter des paramètres `page`/`pageSize` en production.
