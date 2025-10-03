using MGAlienLib;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2
{
    internal class GravityBall : ComponentBase
    {
        private float gravityY = -0.98f;
        private float velocityY = 0;
        public float velocityX = 0;
        private float domainX = 500;
        private float domainY = 500;
        private float elastic = 0.8f;
        private float floatErrorAdjust = 0.95f;
        public SpriteRenderer ball;

        public override void Awake()
        {
            ball = AddComponent<SpriteRenderer>();
            ball.Load("mgcb://test/ball");
            //ball.Load("raw://test/ball.png");
            ball.transform.position = Vector3.Zero;
            ball.transform.localScale = Vector3.One * 50f;
            ball.pivot = new Vector2(0.5f, 0.5f);
        }

        public override void Update()
        {
            velocityY += gravityY;
            if (transform.position.Y < -domainY)
            {
                velocityY = -velocityY * floatErrorAdjust;
            }
            if (transform.position.X < -domainX || transform.position.X > domainX)
            {
                velocityX = -velocityX * elastic ;
            }

            transform.position += new Vector3(velocityX, velocityY, 0);
        }



    }
}
