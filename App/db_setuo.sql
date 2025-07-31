-- Créer la base de données
CREATE DATABASE userdb;

-- Se connecter à la base userdb
\c userdb;

-- Créer la table users
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN NOT NULL DEFAULT true
);

-- Insérer des données de test
INSERT INTO users (id, first_name, last_name, email, created_date, is_active) VALUES
(1,'Jean', 'Dupont', 'jean.dupont@email.com', '2023-01-15', true),
(2,'Marie', 'Martin', 'marie.martin@email.com', '2023-02-20', true),
(3,'Pierre', 'Durand', 'pierre.durand@email.com', '2023-03-10', false),
(4,'Sophie', 'Leclerc', 'sophie.leclerc@email.com', '2023-04-05', true),
(5,'Antoine', 'Moreau', 'antoine.moreau@email.com', '2023-05-12', true),
(6,'Camille', 'Petit', 'camille.petit@email.com', '2023-06-18', false),
(7,'Lucas', 'Roux', 'lucas.roux@email.com', '2023-07-25', true),
(8,'Emma', 'Fournier', 'emma.fournier@email.com', '2023-08-30', true),
(9,'Thomas', 'Girard', 'thomas.girard@email.com', '2023-09-14', true),
(10,'Chloé', 'Bonnet', 'chloe.bonnet@email.com', '2023-10-22', false);

-- Créer des index pour améliorer les performances de recherche
CREATE INDEX idx_users_first_name ON users(first_name);
CREATE INDEX idx_users_last_name ON users(last_name);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_is_active ON users(is_active);