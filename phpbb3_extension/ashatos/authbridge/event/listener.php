<?php
/**
 * ASHATOS Authentication Bridge Extension for phpBB
 *
 * @copyright (c) 2024 ASHATOS Team
 * @license GNU General Public License, version 2 (GPL-2.0)
 */

namespace ashatos\authbridge\event;

use Symfony\Component\EventDispatcher\EventSubscriberInterface;

/**
 * Event listener for ASHATOS Authentication Bridge
 */
class listener implements EventSubscriberInterface
{
    /**
     * Return the events to listen to
     *
     * @return array
     */
    public static function getSubscribedEvents()
    {
        return [
            'core.user_setup' => 'load_language_on_setup',
        ];
    }

    /**
     * Load language file during user setup
     *
     * @param \phpbb\event\data $event Event object
     */
    public function load_language_on_setup($event)
    {
        $lang_set_ext = $event['lang_set_ext'];
        $lang_set_ext[] = [
            'ext_name' => 'ashatos/authbridge',
            'lang_set' => 'common',
        ];
        $event['lang_set_ext'] = $lang_set_ext;
    }
}
