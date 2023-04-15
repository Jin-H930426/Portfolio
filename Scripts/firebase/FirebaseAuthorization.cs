using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Unity.VisualScripting;

namespace JH.Portfolio.Firebase
{
    public class FirebaseAuthorization
    {
        // Firebase authorization
        public FirebaseAuth firebaseAuth { get; private set; } = null;

        // Firebase user data
        public FirebaseUser loginUser { get; private set; } = null;

        // string for debug to check firebase user data
        string _data = "";
        
        public FirebaseAuthorization()
        {
            Debug.Log("Create FirebaseAuthorization");
            Initialize();
        }
        
        /// <summary>
        /// Initialization firebase authorization
        /// </summary>
        public void Initialize()
        {
            Debug.Log("Initialize Check and fix dependencies");
            // Check that Firebase is ready to use
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                // Get dependency status
                var dependencyStatus = task.Result;
                Debug.Log($"dependencyStatus: {dependencyStatus}");
                // If ready, initialize Firebase
                if (dependencyStatus == DependencyStatus.Available)
                {
                    Debug.Log($"Initialize firebaseAuth");
                    // Initialize Firebase
                    // Set the authentication instance object
                    firebaseAuth = FirebaseAuth.DefaultInstance;
                    // Set state changed listener
                    firebaseAuth.StateChanged += AuthStateChanged;
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
            
        }
        
        /// <summary>
        /// Clear firebase authorization
        /// </summary>
        public void Destroy()
        {
            SignOut();
            firebaseAuth.StateChanged -= AuthStateChanged;
            firebaseAuth = null;
        }

        /// <summary>
        /// Handle state changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        void AuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            Debug.Log("Call AuthStateChanged");
            if (firebaseAuth.CurrentUser != loginUser)
            {
                bool signedIn = firebaseAuth.CurrentUser != null;
                if (!signedIn && loginUser != null)
                {
                    Debug.Log($"Signed out {loginUser.UserId}");
                }

                loginUser = firebaseAuth.CurrentUser;
                if (signedIn)
                {
                    Debug.Log($"Signed in {loginUser.UserId}");
                }
            }
            else
            {
                Debug.Log("NonUser");
            }
        }
        /// <summary>
        /// Sign out firebase authorization
        /// </summary>
        public void SignOut()
        {
            firebaseAuth?.SignOut();
        }

        #region User profile

        /// <summary>
        /// Get user profile data from login user
        /// </summary>
        public void GetUserProfile()
        {
            var user = firebaseAuth.CurrentUser;
            if (user == null)
            {
                Debug.LogWarning("Not signed in, unable to get profile");
                return;
            }

            _data = $"Display Name: {user.DisplayName}";
            _data += $"Email: {user.Email}";
            _data += $"Email Verified: {user.IsEmailVerified}";
            _data += $"Photo URL: {user.PhotoUrl}";
            _data += $"Provider ID: {user.ProviderId}";
            _data += $"User ID: {user.UserId}";
            Debug.Log(_data);
        }

        /// <summary>
        /// Get user profile data from login user with provider data
        /// </summary>
        public void GetUserProfileWithProviderData()
        {
            var user = firebaseAuth.CurrentUser;
            if (user == null)
            {
                Debug.LogWarning("Not signed in, unable to get profile");
                return;
            }

            foreach (var userInfo in user.ProviderData)
            {
                _data = $"Display Name: {userInfo.DisplayName}";
                _data += $"Email: {userInfo.Email}";
                _data += $"Photo URL: {userInfo.PhotoUrl}";
                _data += $"Provider ID: {userInfo.ProviderId}";
                _data += $"User ID: {userInfo.UserId}";
                Debug.Log(_data);
            }
        }

        /// <summary>
        /// Update user profile data
        /// </summary>
        /// <param name="profile"></param>
        public void UpdateUserProfile(UserProfile profile)
        {
            // Check if user is signed in
            var user = firebaseAuth.CurrentUser;
            if (user == null)
            {
                Debug.LogWarning("Not signed in, unable to update profile");
                return;
            }

            // Check if profile is null
            if (profile == null)
            {
                Debug.LogWarning("Profile is null, unable to update profile");
                return;
            }

            user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
            });
        }

        #endregion
        #region User account with email and passward

        /// <summary>
        /// Create account with email and passward
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passward"></param>
        public void CreateAccountWithEmailAndPassward(string email, string passward)
        {
            firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, passward).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError($"CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError($"CreateUserWithEmailAndPasswordAsync encountered an error: {task.Exception}");
                    return;
                }

                // Firebase user has been created.
                FirebaseUser newUser = task.Result;
                Debug.Log($"Firebase user created successfully: {newUser.DisplayName} ({newUser.UserId})");
            });
        }

        /// <summary>
        /// Delete account
        /// </summary>
        public void DeleteAccount()
        {
            var user = firebaseAuth.CurrentUser;
            if (user == null)
            {
                Debug.LogWarning("Not signed in, unable to delete account");
                return;
            }

            user.DeleteAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError($"DeleteAsync was canceled.");
                    return;
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError($"DeleteAsync encountered an error: {task.Exception}");
                    return;
                }

                Debug.Log($"User account deleted successfully");
            });
        }

        /// <summary>
        /// Sign in with email and passward
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passward"></param>
        public void SignInWithEmailAndPassward(string email, string passward)
        {
            firebaseAuth.SignInWithEmailAndPasswordAsync(email, passward).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError($"SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError($"SignInWithEmailAndPasswordAsync encountered an error: {task.Exception}");
                    return;
                }

                // Firebase user has been created.
                loginUser = task.Result;
                Debug.Log($"Firebase user signed in successfully: {loginUser.DisplayName} ({loginUser.UserId})");
            });
        }

        /// <summary>
        /// Provider sign in with email and passward
        /// </summary>
        /// <param name="email"></param>
        /// <param name="passward"></param>
        public void AuthorizationProvider(string email, string passward)
        {
            FirebaseUser user = firebaseAuth.CurrentUser;

            // Get auth credentials from the user for re-authentication. The example below shows
            // email and password credentials but there are multiple possible providers,
            // such as GoogleAuthProvider or FacebookAuthProvider.
            Credential credential = EmailAuthProvider.GetCredential(email, passward);

            if (user != null)
            {
                user.ReauthenticateAsync(credential).ContinueWith(task =>
                {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("ReauthenticateAsync was canceled.");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        Debug.LogError("ReauthenticateAsync encountered an error: " + task.Exception);
                        return;
                    }

                    Debug.Log("User reauthenticated successfully.");
                });
            }
        }

        /// <summary>
        /// Set user email address
        /// </summary>
        /// <param name="email"></param>
        public void SetEmailAddress(string email)
        {
            // Check if user is signed in
            var user = firebaseAuth.CurrentUser;
            if (user == null)
            {
                Debug.LogWarning("Not signed in, unable to update email");
                return;
            }

            user.UpdateEmailAsync(email).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateEmailAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateEmailAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User email address updated successfully.");
            });
        }

        /// <summary>
        /// send email verification
        /// </summary>
        public void SendEmailVerification()
        {
            // Check if user is signed in
            var user = firebaseAuth.CurrentUser;
            if (user == null)
            {
                Debug.LogWarning("Not signed in, unable to send email verification");
                return;
            }

            user.SendEmailVerificationAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SendEmailVerificationAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("SendEmailVerificationAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("Email verification sent successfully.");
            });
        }


        /// <summary>
        /// Set user password
        /// </summary>
        /// <param name="password">setting password</param>
        public void SetPassword(string password)
        {
            // Check if user is signed in
            var user = firebaseAuth.CurrentUser;
            if (user == null)
            {
                Debug.LogWarning("Not signed in, unable to update password");
                return;
            }

            user.UpdatePasswordAsync(password).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdatePasswordAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("UpdatePasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User password updated successfully.");
            });
        }

        /// <summary>
        /// Send password reset email
        /// </summary>
        /// <param name="email"></param>
        public void SendResetPasswordEmail(string email)
        {
            firebaseAuth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SendPasswordResetEmailAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("Password reset email sent successfully.");
            });
        }

        #endregion
    }
}