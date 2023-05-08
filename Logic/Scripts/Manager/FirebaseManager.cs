using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JH.Portfolio.Manager
{
    using Firebase;
    public class FirebaseManager
    {
        private FirebaseAuthorization _firebaseAuthorization;
        
        [SerializeField] private string _testEmail;
        [SerializeField] private string _testPassword;
        
        public FirebaseManager()
        {
            Debug.Log($"FirebaseManager initalized.");
            _firebaseAuthorization = new FirebaseAuthorization();
        }

        public void Destroy()
        {
            _firebaseAuthorization.Destroy();
            _firebaseAuthorization = null;
        }
    }
}