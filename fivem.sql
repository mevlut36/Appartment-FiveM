-- phpMyAdmin SQL Dump
-- version 5.2.0
-- https://www.phpmyadmin.net/
--
-- Hôte : 127.0.0.1:3306
-- Généré le : jeu. 25 mai 2023 à 23:53
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
-- Structure de la table `appartment`
--

DROP TABLE IF EXISTS `appartment`;
CREATE TABLE IF NOT EXISTS `appartment` (
  `id_appart` int NOT NULL AUTO_INCREMENT,
  `doors_position` varchar(255) NOT NULL,
  PRIMARY KEY (`id_appart`)
) ENGINE=MyISAM AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- --------------------------------------------------------

--
-- Structure de la table `appart_player`
--

DROP TABLE IF EXISTS `appart_player`;
CREATE TABLE IF NOT EXISTS `appart_player` (
  `id_player` int NOT NULL,
  `id_appart` int NOT NULL,
  `isOpen` tinyint(1) NOT NULL,
  `chest` text NOT NULL,
  PRIMARY KEY (`id_player`),
  KEY `id_appart` (`id_appart`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
