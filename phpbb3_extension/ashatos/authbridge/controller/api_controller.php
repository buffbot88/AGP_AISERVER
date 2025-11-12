<?php
/**
 * ASHATOS Authentication Bridge Extension for phpBB
 *
 * @copyright (c) 2024 ASHATOS Team
 * @license GNU General Public License, version 2 (GPL-2.0)
 */

namespace ashatos\authbridge\controller;

use phpbb\db\driver\driver_interface;
use phpbb\user;
use phpbb\auth\auth;
use phpbb\passwords\manager as passwords_manager;
use phpbb\request\request;
use phpbb\user_loader;
use phpbb\config\config;
use Symfony\Component\HttpFoundation\JsonResponse;
use Symfony\Component\HttpFoundation\Response;

/**
 * API Controller for ASHATOS Authentication Bridge
 * Provides REST API endpoints for login, registration, and session validation
 */
class api_controller
{
    /** @var driver_interface */
    protected $db;

    /** @var user */
    protected $user;

    /** @var auth */
    protected $auth;

    /** @var passwords_manager */
    protected $passwords_manager;

    /** @var request */
    protected $request;

    /** @var user_loader */
    protected $user_loader;

    /** @var config */
    protected $config;

    /** @var string Users table */
    protected $users_table;

    /** @var string Sessions table */
    protected $sessions_table;

    /**
     * Constructor
     */
    public function __construct(
        driver_interface $db,
        user $user,
        auth $auth,
        passwords_manager $passwords_manager,
        request $request,
        user_loader $user_loader,
        config $config
    ) {
        $this->db = $db;
        $this->user = $user;
        $this->auth = $auth;
        $this->passwords_manager = $passwords_manager;
        $this->request = $request;
        $this->user_loader = $user_loader;
        $this->config = $config;
        $this->users_table = $db->get_sql_layer() === 'sqlite3' ? 'phpbb_users' : USERS_TABLE;
        $this->sessions_table = $db->get_sql_layer() === 'sqlite3' ? 'phpbb_sessions' : SESSIONS_TABLE;
    }

    /**
     * Login endpoint - POST /api/auth/login
     * 
     * @return JsonResponse
     */
    public function login()
    {
        // Get JSON input
        $input = $this->get_json_input();
        
        if (!isset($input['username']) || !isset($input['password'])) {
            return $this->json_error('Username and password are required', Response::HTTP_BAD_REQUEST);
        }

        $username = $input['username'];
        $password = $input['password'];

        // Attempt to authenticate
        $result = $this->auth->login($username, $password);

        if ($result['status'] == LOGIN_SUCCESS) {
            // Get user data
            $user_row = $this->get_user_by_username($username);
            
            if ($user_row) {
                // Create session
                $session_id = $this->user->session_id;
                
                return $this->json_response([
                    'success' => true,
                    'message' => 'Login successful',
                    'sessionId' => $session_id,
                    'user' => [
                        'id' => $user_row['user_id'],
                        'username' => $user_row['username'],
                        'email' => $user_row['user_email'],
                        'role' => $this->get_user_role($user_row)
                    ]
                ]);
            }
        }

        return $this->json_error('Invalid username or password', Response::HTTP_UNAUTHORIZED);
    }

    /**
     * Register endpoint - POST /api/auth/register
     * 
     * @return JsonResponse
     */
    public function register()
    {
        // Get JSON input
        $input = $this->get_json_input();
        
        if (!isset($input['username']) || !isset($input['email']) || !isset($input['password'])) {
            return $this->json_error('Username, email, and password are required', Response::HTTP_BAD_REQUEST);
        }

        $username = $input['username'];
        $email = $input['email'];
        $password = $input['password'];

        // Validate input
        if (strlen($username) < 3 || strlen($username) > 20) {
            return $this->json_error('Username must be between 3 and 20 characters', Response::HTTP_BAD_REQUEST);
        }

        if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
            return $this->json_error('Invalid email address', Response::HTTP_BAD_REQUEST);
        }

        if (strlen($password) < 6) {
            return $this->json_error('Password must be at least 6 characters', Response::HTTP_BAD_REQUEST);
        }

        // Check if username already exists
        if ($this->user_exists($username)) {
            return $this->json_error('Username already exists', Response::HTTP_CONFLICT);
        }

        // Check if email already exists
        if ($this->email_exists($email)) {
            return $this->json_error('Email already exists', Response::HTTP_CONFLICT);
        }

        // Create user
        $user_id = $this->create_user($username, $email, $password);

        if ($user_id) {
            // Auto-login the new user
            $this->auth->login($username, $password);
            $session_id = $this->user->session_id;

            // Get user data
            $user_row = $this->get_user_by_id($user_id);

            return $this->json_response([
                'success' => true,
                'message' => 'Registration successful',
                'sessionId' => $session_id,
                'user' => [
                    'id' => $user_row['user_id'],
                    'username' => $user_row['username'],
                    'email' => $user_row['user_email'],
                    'role' => $this->get_user_role($user_row)
                ]
            ], Response::HTTP_CREATED);
        }

        return $this->json_error('Registration failed', Response::HTTP_INTERNAL_SERVER_ERROR);
    }

    /**
     * Validate session endpoint - POST /api/auth/validate
     * 
     * @return JsonResponse
     */
    public function validate()
    {
        // Get JSON input
        $input = $this->get_json_input();
        
        if (!isset($input['sessionId'])) {
            return $this->json_error('Session ID is required', Response::HTTP_BAD_REQUEST);
        }

        $session_id = $input['sessionId'];

        // Validate session
        $user_id = $this->get_user_by_session($session_id);

        if ($user_id) {
            $user_row = $this->get_user_by_id($user_id);

            if ($user_row) {
                return $this->json_response([
                    'success' => true,
                    'user' => [
                        'id' => $user_row['user_id'],
                        'username' => $user_row['username'],
                        'email' => $user_row['user_email'],
                        'role' => $this->get_user_role($user_row)
                    ]
                ]);
            }
        }

        return $this->json_error('Invalid or expired session', Response::HTTP_UNAUTHORIZED);
    }

    /**
     * Logout endpoint - POST /api/auth/logout
     * 
     * @return JsonResponse
     */
    public function logout()
    {
        // Get JSON input
        $input = $this->get_json_input();
        
        if (!isset($input['sessionId'])) {
            return $this->json_error('Session ID is required', Response::HTTP_BAD_REQUEST);
        }

        $session_id = $input['sessionId'];

        // Delete session
        $sql = 'DELETE FROM ' . $this->sessions_table . '
                WHERE session_id = \'' . $this->db->sql_escape($session_id) . '\'';
        $this->db->sql_query($sql);

        return $this->json_response([
            'success' => true,
            'message' => 'Logout successful'
        ]);
    }

    /**
     * Get JSON input from request
     * 
     * @return array
     */
    protected function get_json_input()
    {
        $content = file_get_contents('php://input');
        $data = json_decode($content, true);
        return $data ?: [];
    }

    /**
     * Check if username exists
     * 
     * @param string $username
     * @return bool
     */
    protected function user_exists($username)
    {
        $sql = 'SELECT user_id FROM ' . $this->users_table . '
                WHERE username_clean = \'' . $this->db->sql_escape(utf8_clean_string($username)) . '\'';
        $result = $this->db->sql_query($sql);
        $row = $this->db->sql_fetchrow($result);
        $this->db->sql_freeresult($result);
        
        return (bool) $row;
    }

    /**
     * Check if email exists
     * 
     * @param string $email
     * @return bool
     */
    protected function email_exists($email)
    {
        $sql = 'SELECT user_id FROM ' . $this->users_table . '
                WHERE user_email = \'' . $this->db->sql_escape($email) . '\'';
        $result = $this->db->sql_query($sql);
        $row = $this->db->sql_fetchrow($result);
        $this->db->sql_freeresult($result);
        
        return (bool) $row;
    }

    /**
     * Create new user
     * 
     * @param string $username
     * @param string $email
     * @param string $password
     * @return int|false User ID on success, false on failure
     */
    protected function create_user($username, $email, $password)
    {
        // Hash password using phpBB's password manager
        $hashed_password = $this->passwords_manager->hash($password);

        // Prepare user data
        $user_row = [
            'username' => $username,
            'username_clean' => utf8_clean_string($username),
            'user_email' => $email,
            'user_password' => $hashed_password,
            'user_type' => USER_NORMAL,
            'user_regdate' => time(),
            'user_lang' => $this->config['default_lang'],
            'user_timezone' => $this->config['board_timezone'],
            'user_dateformat' => $this->config['default_dateformat'],
            'user_style' => (int) $this->config['default_style'],
            'user_actkey' => '',
            'user_ip' => $this->request->server('REMOTE_ADDR'),
            'user_form_salt' => unique_id(),
            'user_new' => 0,
            'user_inactive_reason' => 0,
            'user_inactive_time' => 0,
        ];

        // Insert user
        $sql = 'INSERT INTO ' . $this->users_table . ' ' . $this->db->sql_build_array('INSERT', $user_row);
        $this->db->sql_query($sql);
        
        return $this->db->sql_nextid();
    }

    /**
     * Get user by username
     * 
     * @param string $username
     * @return array|false
     */
    protected function get_user_by_username($username)
    {
        $sql = 'SELECT user_id, username, user_email, user_type, group_id
                FROM ' . $this->users_table . '
                WHERE username_clean = \'' . $this->db->sql_escape(utf8_clean_string($username)) . '\'';
        $result = $this->db->sql_query($sql);
        $row = $this->db->sql_fetchrow($result);
        $this->db->sql_freeresult($result);
        
        return $row;
    }

    /**
     * Get user by ID
     * 
     * @param int $user_id
     * @return array|false
     */
    protected function get_user_by_id($user_id)
    {
        $sql = 'SELECT user_id, username, user_email, user_type, group_id
                FROM ' . $this->users_table . '
                WHERE user_id = ' . (int) $user_id;
        $result = $this->db->sql_query($sql);
        $row = $this->db->sql_fetchrow($result);
        $this->db->sql_freeresult($result);
        
        return $row;
    }

    /**
     * Get user ID by session ID
     * 
     * @param string $session_id
     * @return int|false
     */
    protected function get_user_by_session($session_id)
    {
        $sql = 'SELECT session_user_id FROM ' . $this->sessions_table . '
                WHERE session_id = \'' . $this->db->sql_escape($session_id) . '\'
                AND session_time >= ' . (time() - (int) $this->config['session_length']);
        $result = $this->db->sql_query($sql);
        $row = $this->db->sql_fetchrow($result);
        $this->db->sql_freeresult($result);
        
        return $row ? (int) $row['session_user_id'] : false;
    }

    /**
     * Get user role
     * 
     * @param array $user_row
     * @return string
     */
    protected function get_user_role($user_row)
    {
        // Check if user is founder/admin
        if ($user_row['user_type'] == USER_FOUNDER) {
            return 'Admin';
        }

        // Check if user is in administrators group (group_id 5)
        if (isset($user_row['group_id']) && $user_row['group_id'] == 5) {
            return 'Admin';
        }

        return 'User';
    }

    /**
     * Create JSON response
     * 
     * @param array $data
     * @param int $status
     * @return JsonResponse
     */
    protected function json_response($data, $status = Response::HTTP_OK)
    {
        return new JsonResponse($data, $status, [
            'Content-Type' => 'application/json',
            'Access-Control-Allow-Origin' => '*',
            'Access-Control-Allow-Methods' => 'GET, POST, PUT, DELETE, OPTIONS',
            'Access-Control-Allow-Headers' => 'Content-Type, Authorization',
        ]);
    }

    /**
     * Create JSON error response
     * 
     * @param string $message
     * @param int $status
     * @return JsonResponse
     */
    protected function json_error($message, $status = Response::HTTP_BAD_REQUEST)
    {
        return $this->json_response([
            'success' => false,
            'message' => $message
        ], $status);
    }
}
