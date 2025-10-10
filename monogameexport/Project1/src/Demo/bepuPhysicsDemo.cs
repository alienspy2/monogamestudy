
using MGAlienLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Project1
{
    internal class bepuPhysicsDemo : _3DDemoBase
    {
        public override void Awake()
        {
            base.Awake();
            //game.physicsManager.test_addStaticBox();

            //var obj = hierarchyManager.CreateGameObject("static ball", transform);
            //obj.transform.position = new Vector3(0, 0, 0);
            //obj.transform.scale = new Vector3(5,5,5);
            //var ball = obj.AddComponent<StaticBall>();

            var groundObj = hierarchyManager.CreateGameObject("ground", transform);
            groundObj.transform.scale = new Vector3(10, 1, 10);
            var groundRdr = groundObj.AddComponent<MeshRenderer>();
            groundRdr.Load("raw://EditorResources/box.glb");
            groundRdr.LoadMaterial("MG/3D/Lit");
            groundRdr.BreakMaterialSharing();
            groundRdr.material.asset.SetColor("_BaseColor", Color.Gray);
            var groundCollider = groundObj.AddComponent<BoxCollider>();

            var wallObj1 = hierarchyManager.CreateGameObject("wall1", transform);
            wallObj1.transform.position = new Vector3(0, 1, -5);
            wallObj1.transform.scale = new Vector3(10, 2, 1);
            var wallRdr1 = wallObj1.AddComponent<MeshRenderer>();
            wallRdr1.Load("raw://EditorResources/box.glb");
            wallRdr1.ShareMaterial(groundRdr.material);
            var wallCollider1 = wallRdr1.AddComponent<BoxCollider>();

            var wallObj2 = hierarchyManager.CreateGameObject("wall2", transform);
            wallObj2.transform.position = new Vector3(0, 1, 5);
            wallObj2.transform.scale = new Vector3(10, 2, 1);
            var wallRdr2 = wallObj2.AddComponent<MeshRenderer>();
            wallRdr2.Load("raw://EditorResources/box.glb");
            wallRdr2.ShareMaterial(groundRdr.material);
            var wallCollider2 = wallRdr2.AddComponent<BoxCollider>();

            var wallObj3 = hierarchyManager.CreateGameObject("wall3", transform);
            wallObj3.transform.position = new Vector3(-5, 1, 0);
            wallObj3.transform.scale = new Vector3(1, 2, 10);
            var wallRdr3 = wallObj3.AddComponent<MeshRenderer>();
            wallRdr3.Load("raw://EditorResources/box.glb");
            wallRdr3.ShareMaterial(groundRdr.material);
            var wallCollider3 = wallRdr3.AddComponent<BoxCollider>();

            var wallObj4 = hierarchyManager.CreateGameObject("wall4", transform);
            wallObj4.transform.position = new Vector3(5, 1, 0);
            wallObj4.transform.scale = new Vector3(1, 2, 10);
            var wallRdr4 = wallObj4.AddComponent<MeshRenderer>();
            wallRdr4.Load("raw://EditorResources/box.glb");
            wallRdr4.ShareMaterial(groundRdr.material);
            var wallCollider4 = wallRdr4.AddComponent<BoxCollider>();
        }

        public override void Update()
        {
            base.Update();

            var random = new System.Random();

            if (inputManager.IsPressed(Keys.D1))
            {

                var obj = hierarchyManager.CreateGameObject("ball", transform);
                obj.transform.position = new Vector3(random.NextSingle() * 3f,
                                                    10,
                                                    random.NextSingle() * 3f);
                var ball = obj.AddComponent<RigidbodyBall>();
            }

            if (inputManager.IsPressed(Keys.D2))
            {

                var obj = hierarchyManager.CreateGameObject("ball", transform);
                obj.transform.position = new Vector3(random.NextSingle() * 3f,
                                                    10,
                                                    random.NextSingle() * 3f);
                var ball = obj.AddComponent<RigidbodyBox>();
            }
        }
    }
}
