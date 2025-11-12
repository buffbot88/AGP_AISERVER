<?php
/**
 * ASHATOS Authentication Bridge Extension for phpBB
 *
 * @copyright (c) 2024 ASHATOS Team
 * @license GNU General Public License, version 2 (GPL-2.0)
 */

/**
 * DO NOT CHANGE
 */
if (!defined('IN_PHPBB'))
{
    exit;
}

if (empty($lang) || !is_array($lang))
{
    $lang = [];
}

$lang = array_merge($lang, [
    'ASHATOS_AUTHBRIDGE_TITLE'       => 'ASHATOS Authentication Bridge',
    'ASHATOS_AUTHBRIDGE_DESCRIPTION' => 'Provides REST API endpoints for authentication with the ASHATOS AI Game Server',
    'ASHATOS_AUTHBRIDGE_VERSION'     => 'Version 1.0.0',
]);
