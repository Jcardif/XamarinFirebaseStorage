﻿using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using System;
using Android.Content;
using Android.Runtime;
using Android.Graphics;
using Android.Provider;
using Java.IO;
using Firebase.Storage;
using Firebase;
using Java.Lang;
using Android.Gms.Tasks;
using Square.Picasso;
using Android.Media;

namespace XamarinFirebaseStorage
{
    [Activity(Label = "XamarinFirebaseStorage", MainLauncher = true, Icon = "@drawable/icon", Theme ="@style/Theme.AppCompat.Light.NoActionBar")]
    public class MainActivity : AppCompatActivity, IOnProgressListener, IOnSuccessListener,IOnFailureListener
    {
       private Button chooseBtn, uploadBtn;
       private ImageView mImageView;

        private Android.Net.Uri filePath;
        private const int PICK_IMAGE_REQUEST = 71;

        private ProgressDialog progressDialog;
        FirebaseStorage storage;
        StorageReference storeref;
        private string imageName;
        Android.Net.Uri downloadUrl;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView (Resource.Layout.Main);

            FirebaseApp.InitializeApp(this);
            storage = FirebaseStorage.Instance;
            storeref = storage.GetReferenceFromUrl("gs://aeroplaneimages.appspot.com");
            
            chooseBtn = FindViewById<Button>(Resource.Id.choosePhotobtn);
            uploadBtn = FindViewById<Button>(Resource.Id.uploadPhotobtn);
            mImageView = FindViewById<ImageView>(Resource.Id.imageView);

            chooseBtn.Click += delegate
            {
                ChooseImage();
            };
            uploadBtn.Click += delegate
            {
                UploadImage();
            };
        }

        private void UploadImage()
        {
            if (filePath != null)
            {
                progressDialog = new ProgressDialog(this);
                progressDialog.SetTitle("Uploading....");
                progressDialog.Window.SetType(Android.Views.WindowManagerTypes.SystemAlert);
                progressDialog.Show();

                var images = storeref.Child("aeroplaneImages/" + Guid.NewGuid().ToString());
                images.PutFile(filePath)
                    .AddOnProgressListener(this)
                    .AddOnSuccessListener(this)
                    .AddOnFailureListener(this);
            }
        }

        private void ChooseImage()
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Select Image"), PICK_IMAGE_REQUEST);
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if(requestCode==PICK_IMAGE_REQUEST && resultCode==Result.Ok && data!=null && data.Data != null)
            {
                filePath = data.Data;
                try
                {
                     imageName= filePath.LastPathSegment.ToString();
                    Toast.MakeText(this, "" +imageName , ToastLength.Short).Show();

                            //either
                   // Bitmap bitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, filePath);
                   //mimageView.SetImageBitmap(bitmap)
                                //or 

                    mImageView.SetImageURI(filePath);                    
                }
                catch(IOException ex)
                {
                    ex.PrintStackTrace();
                }
            }
        }

        public void OnProgress(Java.Lang.Object snapshot)
        {
            var taskSnapshot = (UploadTask.TaskSnapshot)snapshot;
            double progress = (100.0 * taskSnapshot.BytesTransferred / taskSnapshot.TotalByteCount);
            progressDialog.SetMessage("Uploaded " + (int)progress + "%");

            downloadUrl = taskSnapshot.DownloadUrl;
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            progressDialog.Dismiss();
            Toast.MakeText(this, "Upload Succcessful", ToastLength.Short).Show();
            mImageView.SetImageBitmap(null);

            Picasso.With(this)
                .Load(downloadUrl)
                .Into(mImageView);
        }

        public void OnFailure(Java.Lang.Exception e)
        {
            progressDialog.Dismiss();
            Toast.MakeText(this, ""+e.Message, ToastLength.Short).Show();
        }
    }
}

