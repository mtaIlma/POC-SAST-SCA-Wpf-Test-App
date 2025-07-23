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
INSERT INTO users (first_name, last_name, email, created_date, is_active) VALUES
('Jean', 'Dupont', 'jean.dupont@email.com', '2023-01-15', true),
('Marie', 'Martin', 'marie.martin@email.com', '2023-02-20', true),
('Pierre', 'Durand', 'pierre.durand@email.com', '2023-03-10', false),
('Sophie', 'Leclerc', 'sophie.leclerc@email.com', '2023-04-05', true),
('Antoine', 'Moreau', 'antoine.moreau@email.com', '2023-05-12', true),
('Camille', 'Petit', 'camille.petit@email.com', '2023-06-18', false),
('Lucas', 'Roux', 'lucas.roux@email.com', '2023-07-25', true),
('Emma', 'Fournier', 'emma.fournier@email.com', '2023-08-30', true),
('Thomas', 'Girard', 'thomas.girard@email.com', '2023-09-14', true),
('Chloé', 'Bonnet', 'chloe.bonnet@email.com', '2023-10-22', false);

-- Créer des index pour améliorer les performances de recherche
CREATE INDEX idx_users_first_name ON users(first_name);
CREATE INDEX idx_users_last_name ON users(last_name);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_is_active ON users(is_active);