# TerminalApi - Documentation du Projet

## Architecture de la Base de Données

Cette section présente l'architecture de la base de données du projet TerminalApi, incluant le Modèle Conceptuel de Données (MCD) et le Modèle Logique de Données (MLD).

### Modèle Conceptuel de Données (MCD)

Le MCD représente la structure conceptuelle de notre système de gestion de formations et réservations en ligne. Il illustre les entités principales et leurs relations métier.

```mermaid
erDiagram
    USER {
        string Id PK
        string FirstName
        string LastName
        string Email
        enum Gender
        string ImgUrl
        string Description
        string Title
        string LinkedinUrl
        string GithubUrl
        datetime DateOfBirth
        datetime CreatedAt
        datetime LastModifiedAt
        datetime LastLogginAt
        boolean IsBanned
        datetime BannedUntilDate
    }

    ADDRESS {
        guid Id PK
        int StreetNumber
        string Street
        string StreetLine2
        string City
        string State
        string PostalCode
        string Country
        string UserId FK
        enum AddressType
    }

    SLOT {
        guid Id PK
        datetime StartAt
        datetime EndAt
        datetime CreatedAt
        string CreatedById FK
        decimal Price
        int Reduction
        enum Type
    }

    BOOKING {
        guid Id PK
        guid SlotId FK
        string BookedById FK
        datetime CreatedAt
        guid OrderId FK
        string Subject
        string Description
        enum TypeHelp
        json Communications
    }

    ORDER {
        guid Id PK
        string OrderNumber
        datetime PaymentDate
        string CheckoutID
        datetime CreatedAt
        datetime UpdatedAt
        enum Status
        string PaymentMethod
        string BookerId FK
        decimal TVARate
        string PaymentIntent
        datetime CheckoutExpiredAt
    }

    FORMATION {
        guid Id PK
        string Company
        string Title
        datetime StartAt
        datetime EndAt
        string UserId FK
        string City
        string Country
    }

    NOTIFICATION {
        guid Id PK
        string Description
        enum Type
        boolean IsRead
        datetime CreatedAt
        string SenderId FK
        string RecipientId FK
        guid BookingId FK
        guid OrderId FK
    }

    CURSUS {
        guid Id PK
        string Name
        string Description
        guid LevelId FK
        guid CategoryId FK
        datetime CreatedAt
        datetime UpdatedAt
    }

    LEVEL {
        guid Id PK
        string Name
        string Icon
        string Color
    }

    CATEGORY {
        guid Id PK
        string Name
        string Icon
        string Color
    }

    TVARATE {
        guid Id PK
        datetime StartAt
        decimal Rate
    }

    ROLE {
        string Id PK
        string Name
    }

    USER ||--o{ ADDRESS : "possede"
    USER ||--o{ SLOT : "cree"
    USER ||--o{ BOOKING : "reserve"
    USER ||--o{ ORDER : "commande"
    USER ||--o{ FORMATION : "suit"
    USER ||--o{ NOTIFICATION : "envoie"
    USER ||--o{ NOTIFICATION : "recoit"

    SLOT ||--o| BOOKING : "est_reserve"
    BOOKING }o--|| ORDER : "appartient_a"
    BOOKING ||--o{ NOTIFICATION : "genere"
    ORDER ||--o{ NOTIFICATION : "genere"

    CURSUS }o--|| LEVEL : "a_niveau"
    CURSUS }o--|| CATEGORY : "appartient_categorie"
```

#### Description des Entités Principales

**USER (Utilisateur)** : Entité centrale du système représentant les utilisateurs (étudiants, formateurs, administrateurs). Chaque utilisateur peut avoir plusieurs rôles et possède un profil complet avec informations personnelles et professionnelles.

**SLOT (Créneau)** : Représente les créneaux horaires disponibles créés par les formateurs. Chaque créneau a un prix, peut avoir une réduction et est typé selon le service proposé.

**BOOKING (Réservation)** : Entité de liaison entre un utilisateur et un créneau. Une réservation contient les détails de la demande d'aide et peut inclure des communications via chat.

**ORDER (Commande)** : Regroupe une ou plusieurs réservations pour le processus de paiement. Gère le cycle de vie commercial avec statuts, méthodes de paiement et TVA.

**CURSUS** : Représente les parcours de formation structurés par niveaux et catégories, permettant une organisation pédagogique cohérente.

### Modèle Logique de Données (MLD)

Le MLD présente l'implémentation technique de la base de données avec les tables, types de données et contraintes réelles.

```mermaid
erDiagram
    AspNetUsers {
        nvarchar Id PK "Identifiant unique utilisateur"
        nvarchar FirstName "Prénom"
        nvarchar LastName "Nom de famille"
        nvarchar Email "Adresse email"
        int Gender "Genre (enum)"
        nvarchar ImgUrl "URL image profil"
        text Description "Description utilisateur"
        nvarchar Title "Titre professionnel"
        nvarchar LinkedinUrl "URL LinkedIn"
        nvarchar GithubUrl "URL GitHub"
        timestamptz DateOfBirth "Date de naissance"
        timestamptz CreatedAt "Date création"
        timestamptz LastModifiedAt "Dernière modification"
        timestamptz LastLogginAt "Dernière connexion"
        boolean IsBanned "Utilisateur banni"
        timestamptz BannedUntilDate "Date fin bannissement"
        nvarchar UserName "Nom utilisateur"
        nvarchar NormalizedUserName "Nom utilisateur normalisé"
        nvarchar NormalizedEmail "Email normalisé"
        boolean EmailConfirmed "Email confirmé"
        nvarchar PasswordHash "Hash mot de passe"
        nvarchar SecurityStamp "Stamp sécurité"
        nvarchar ConcurrencyStamp "Stamp concurrence"
        nvarchar PhoneNumber "Numéro téléphone"
        boolean PhoneNumberConfirmed "Téléphone confirmé"
        boolean TwoFactorEnabled "2FA activé"
        datetimeoffset LockoutEnd "Fin verrouillage"
        boolean LockoutEnabled "Verrouillage activé"
        int AccessFailedCount "Tentatives échouées"
    }

    Addresses {
        uuid Id PK "Identifiant adresse"
        int StreetNumber "Numéro rue"
        nvarchar Street "Nom rue"
        nvarchar StreetLine2 "Complément adresse"
        nvarchar City "Ville"
        nvarchar State "État/Région"
        nvarchar PostalCode "Code postal"
        nvarchar Country "Pays"
        nvarchar UserId FK "Référence utilisateur"
        int AddressType "Type adresse (enum)"
    }

    Slots {
        uuid Id PK "Identifiant créneau"
        timestamptz StartAt "Début créneau"
        timestamptz EndAt "Fin créneau"
        timestamptz CreatedAt "Date création"
        nvarchar CreatedById FK "Créateur créneau"
        decimal Price "Prix créneau"
        int Reduction "Réduction pourcentage"
        int Type "Type créneau (enum)"
    }

    Bookings {
        uuid Id PK "Identifiant réservation"
        uuid SlotId FK "Référence créneau"
        nvarchar BookedById FK "Référence utilisateur"
        timestamptz CreatedAt "Date création"
        uuid OrderId FK "Référence commande"
        nvarchar Subject "Sujet réservation"
        text Description "Description détaillée"
        int TypeHelp "Type aide (enum)"
        jsonb Communications "Messages chat"
    }

    Orders {
        uuid Id PK "Identifiant commande"
        nvarchar OrderNumber "Numéro commande"
        timestamptz PaymentDate "Date paiement"
        nvarchar CheckoutID "ID checkout"
        timestamptz CreatedAt "Date création"
        timestamptz UpdatedAt "Date mise à jour"
        int Status "Statut commande (enum)"
        nvarchar PaymentMethod "Méthode paiement"
        nvarchar BookerId FK "Référence client"
        decimal TVARate "Taux TVA"
        nvarchar PaymentIntent "Intent paiement"
        timestamptz CheckoutExpiredAt "Expiration checkout"
    }

    Formations {
        uuid Id PK "Identifiant formation"
        nvarchar Company "Entreprise"
        nvarchar Title "Titre formation"
        timestamptz StartAt "Date début"
        timestamptz EndAt "Date fin"
        nvarchar UserId FK "Référence utilisateur"
        nvarchar City "Ville"
        nvarchar Country "Pays"
    }

    Notifications {
        uuid Id PK "Identifiant notification"
        nvarchar Description "Description"
        int Type "Type notification (enum)"
        boolean IsRead "Lu/non lu"
        timestamptz CreatedAt "Date création"
        nvarchar SenderId FK "Expéditeur"
        nvarchar RecipientId FK "Destinataire"
        uuid BookingId FK "Référence réservation"
        uuid OrderId FK "Référence commande"
    }

    Cursus {
        uuid Id PK "Identifiant cursus"
        nvarchar Name "Nom cursus"
        nvarchar Description "Description"
        uuid LevelId FK "Référence niveau"
        uuid CategoryId FK "Référence catégorie"
        timestamptz CreatedAt "Date création"
        timestamptz UpdatedAt "Date mise à jour"
    }

    Levels {
        uuid Id PK "Identifiant niveau"
        nvarchar Name "Nom niveau"
        nvarchar Icon "Icône"
        nvarchar Color "Couleur"
    }

    Categories {
        uuid Id PK "Identifiant catégorie"
        nvarchar Name "Nom catégorie"
        nvarchar Icon "Icône"
        nvarchar Color "Couleur"
    }

    TVARates {
        uuid Id PK "Identifiant taux TVA"
        timestamptz StartAt "Date application"
        decimal Rate "Taux TVA"
    }

    AspNetRoles {
        nvarchar Id PK "Identifiant rôle"
        nvarchar Name "Nom rôle"
        nvarchar NormalizedName "Nom normalisé"
        nvarchar ConcurrencyStamp "Stamp concurrence"
    }

    AspNetUserRoles {
        nvarchar UserId PK,FK "Référence utilisateur"
        nvarchar RoleId PK,FK "Référence rôle"
    }

    AspNetUsers ||--o{ Addresses : "UserId"
    AspNetUsers ||--o{ Slots : "CreatedById"
    AspNetUsers ||--o{ Bookings : "BookedById"
    AspNetUsers ||--o{ Orders : "BookerId"
    AspNetUsers ||--o{ Formations : "UserId"
    AspNetUsers ||--o{ Notifications : "SenderId"
    AspNetUsers ||--o{ Notifications : "RecipientId"
    AspNetUsers ||--o{ AspNetUserRoles : "UserId"

    Slots ||--o| Bookings : "SlotId"
    Bookings }o--|| Orders : "OrderId"
    Bookings ||--o{ Notifications : "BookingId"
    Orders ||--o{ Notifications : "OrderId"

    Cursus }o--|| Levels : "LevelId"
    Cursus }o--|| Categories : "CategoryId"

    AspNetRoles ||--o{ AspNetUserRoles : "RoleId"
```

#### Spécifications Techniques

**Système d'Authentification** : Utilisation d'ASP.NET Core Identity avec tables `AspNetUsers`, `AspNetRoles` et `AspNetUserRoles` pour la gestion des utilisateurs et des autorisations.

**Types de Données** :

- `uuid` : Identifiants uniques pour les entités métier
- `timestamptz` : Horodatage avec fuseau horaire pour PostgreSQL
- `decimal(18,2)` : Précision monétaire pour les prix et taux
- `jsonb` : Stockage JSON binaire pour les communications chat
- `text` : Texte de longueur variable pour les descriptions

**Contraintes d'Intégrité** :

- Clés étrangères avec actions de suppression configurées (`CASCADE`, `RESTRICT`, `SET NULL`)
- Contraintes de longueur sur les champs texte
- Valeurs par défaut pour les champs optionnels
- Index sur les clés étrangères pour optimiser les performances

**Particularités du Modèle** :

- Relation 1:1 entre `Slot` et `Booking` (un créneau ne peut être réservé qu'une fois)
- Relation N:M entre `User` et `Role` via la table de liaison `AspNetUserRoles`
- Système de notifications polymorphe pouvant référencer différents types d'entités
- Gestion des adresses multiples par utilisateur avec typage (domicile, travail, facturation)
- Système de réductions et calculs de prix avec propriétés calculées

### Règles de Gestion

1. **Utilisateurs** : Un utilisateur peut avoir plusieurs rôles, plusieurs adresses et peut être temporairement banni
2. **Créneaux** : Seuls les formateurs peuvent créer des créneaux, qui ont un prix de base et une réduction optionnelle
3. **Réservations** : Une réservation lie un étudiant à un créneau spécifique et doit être associée à une commande pour être validée
4. **Commandes** : Regroupent une ou plusieurs réservations avec gestion du processus de paiement et application de la TVA
5. **Formations** : Historique des formations suivies par les utilisateurs avec localisation géographique
6. **Cursus** : Organisation pédagogique avec niveaux et catégories prédéfinis lors de l'initialisation de la base

---

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
