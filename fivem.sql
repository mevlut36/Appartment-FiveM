-- phpMyAdmin SQL Dump
-- version 5.2.0
-- https://www.phpmyadmin.net/
--
-- Hôte : 127.0.0.1:3306
-- Généré le : mer. 31 mai 2023 à 15:22
-- Version du serveur : 8.0.31
-- Version de PHP : 8.0.26

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de données : `fivem`
--

-- --------------------------------------------------------

--
-- Structure de la table `appart_player`
--

DROP TABLE IF EXISTS `appart_player`;
CREATE TABLE IF NOT EXISTS `appart_player` (
  `id_player` int NOT NULL,
  `id_property` int NOT NULL,
  `isOpen` tinyint(1) NOT NULL,
  `chest` text NOT NULL,
  `price` int NOT NULL,
  `booking` text NOT NULL,
  PRIMARY KEY (`id_player`),
  KEY `id_appart` (`id_property`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `appart_player`
--

INSERT INTO `appart_player` (`id_player`, `id_property`, `isOpen`, `chest`, `price`, `booking`) VALUES
(1, 1, 1, '[\"1\"]', 100000, '1'),
(2, 1, 1, '[\"1\"]', 150000, '2'),
(3, 1, 1, '[\"1\"]', 175000, '3'),
(4, 2, 1, '[\"1\"]', 150000, '1'),
(5, 2, 1, '[\"1\"]', 185000, '1');

-- --------------------------------------------------------

--
-- Structure de la table `property`
--

DROP TABLE IF EXISTS `property`;
CREATE TABLE IF NOT EXISTS `property` (
  `id_property` int NOT NULL AUTO_INCREMENT,
  `doors_position` varchar(255) NOT NULL,
  `dress_position` varchar(255) NOT NULL,
  PRIMARY KEY (`id_property`)
) ENGINE=MyISAM AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Déchargement des données de la table `property`
--

INSERT INTO `property` (`id_property`, `doors_position`) VALUES
(1, '[[-774.9041, 311.9501, 86.0722], [-783.5264, 323.6873, 212.6569]]', '[-793.629,326.9281,210.9505]'),
(2, '[[-770.6283, 312.2651, 85.85048], [-774.0837, 331.1519, 207.6208]]', '[-793.629,326.9281,210.9505]');
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
