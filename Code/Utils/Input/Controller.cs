using ChromaCore.Code.Scenes;
using Microsoft.Xna.Framework.Input;
using System.Xml;

namespace ChromaCore.Code.Utils.Input
{
    /// <summary>
    /// <para>Basic class for managing keyboard and controller input</para>
    /// </summary>
    public class Controller
    {
        public int id;
        private const int KeyboardPort = -1;

        protected const int Inputbuffer = 3;
        protected int motionInputBuffer = 20;
        private const float deadzone = 0.4f;

        public const int NumKeys = 15;

        public const int Key_MenuConfirm = 0;
        public const int Key_MenuBack = 1;
        public const int Key_Start = 2;
        public const int Key_Select = 3;
        public const int Key_Up = 4;
        public const int Key_Down = 5;
        public const int Key_Left = 6;
        public const int Key_Right = 7;
        public const int Key_Light = 8;
        public const int Key_Medium = 9;
        public const int Key_Heavy = 10;
        public const int Key_Grab = 11;
        public const int Key_Dash = 12;
        public const int Key_MenuLB = 13;
        public const int Key_MenuRB = 14;

        public void SetBinds()
        {
            keyBind[Key_MenuConfirm] = Keys.Enter;
            keyBind[Key_MenuBack] = Keys.Escape;
            keyBind[Key_Start] = Keys.Escape;
            keyBind[Key_Select] = Keys.Tab;
            keyBind[Key_Up] = Keys.W;
            keyBind[Key_Down] = Keys.S;
            keyBind[Key_Left] = Keys.A;
            keyBind[Key_Right] = Keys.D;
            keyBind[Key_Light] = Keys.J;
            keyBind[Key_Medium] = Keys.K;
            keyBind[Key_Heavy] = Keys.L;
            keyBind[Key_Grab] = Keys.U;
            keyBind[Key_Dash] = Keys.I;
            keyBind[Key_MenuLB] = Keys.D1;
            keyBind[Key_MenuRB] = Keys.D2;

            padBind[Key_MenuConfirm] = Buttons.A;
            padBind[Key_MenuBack] = Buttons.B;
            padBind[Key_Start] = Buttons.Start;
            padBind[Key_Select] = Buttons.Back;
            padBind[Key_Up] = Buttons.LeftThumbstickUp;
            padBind[Key_Down] = Buttons.LeftThumbstickDown;
            padBind[Key_Left] = Buttons.LeftThumbstickLeft;
            padBind[Key_Right] = Buttons.LeftThumbstickRight;
            padBind[Key_Light] = CorrectedButton(Buttons.X);
            padBind[Key_Medium] = CorrectedButton(Buttons.A);
            padBind[Key_Heavy] = CorrectedButton(Buttons.B);
            padBind[Key_Grab] = CorrectedButton(Buttons.Y);
            padBind[Key_Dash] = Buttons.LeftTrigger;
            padBind[Key_MenuLB] = Buttons.LeftShoulder;
            padBind[Key_MenuRB] = Buttons.RightShoulder;
        }

        protected Keys[] keyBind;
        protected Buttons[] padBind;
        protected int[] keyPressed;
        protected bool[] keyDown;
        protected int[] keyReleased;

        private int dTapDashBufferR;
        private int dTapDashBufferL;
        protected int[] dTapDown = new int[2];
        protected int[] qcForward = new int[2];
        protected int[] qcBackwards = new int[2];
        protected int[] hcForward = new int[3];
        protected int[] hcBackwards = new int[3];
        protected int[] chargeDown = new int[2];
        protected int[] chargeBack = new int[2];
        protected int[] dpForward = new int[3];
        protected int[] dpBackwards = new int[3];

        public Keys[] GetKeyBind => (Keys[])keyBind.Clone();
        public Buttons[] GetPadBind => (Buttons[])padBind.Clone();

        public Controller() { }

        public Controller(int ID)
        {
            keyBind = new Keys[NumKeys];
            padBind = new Buttons[NumKeys];
            keyPressed = new int[NumKeys];
            keyDown = new bool[NumKeys];
            keyReleased = new int[NumKeys];

            id = ID;

            SetBinds();
        }

        public Buttons CorrectedButton(Buttons originalButton)
        {
            if (GamePad.GetCapabilities(id).DisplayName != null && GamePad.GetCapabilities(id).DisplayName.Contains("Nintendo"))
            {
                if (originalButton == Buttons.A) originalButton = Buttons.B;
                else if (originalButton == Buttons.B) originalButton = Buttons.A;
                else if (originalButton == Buttons.X) originalButton = Buttons.Y;
                else if (originalButton == Buttons.Y) originalButton = Buttons.X;
            }

            return originalButton;
        }

        public static Buttons CorrectedButton(int controllerID, Buttons originalButton)
        {
            if (GamePad.GetCapabilities(controllerID).DisplayName.Contains("Nintendo"))
            {
                if (originalButton == Buttons.A) originalButton = Buttons.B;
                else if (originalButton == Buttons.B) originalButton = Buttons.A;
                else if (originalButton == Buttons.X) originalButton = Buttons.Y;
                else if (originalButton == Buttons.Y) originalButton = Buttons.X;
            }

            return originalButton;
        }

        public virtual void UpdateKeys(int direction)
        {
            UpdateKeys(Keyboard.GetState(), GamePad.GetState(id), Mouse.GetState(), direction);
        }

        public virtual void UpdateKeys(InputState inputs, int direction)
        {
            UpdateKeys(inputs.keyState, inputs.padState, inputs.mouseState, direction);
        }

        public virtual void UpdateKeys(bool[] keys, int direction)
        {
            for (int i = 0; i < keyPressed.Length; i++)
            {
                if (Game.Instance.Scene is InGame scene)
                {
                    if (scene.hitpause <= 0)
                    {
                        if (keyPressed[i] > 0) keyPressed[i]--;
                        if (keyReleased[i] > 0) keyReleased[i]--;
                    }
                }
                else
                {
                    if (keyPressed[i] > 0) keyPressed[i]--;
                    if (keyReleased[i] > 0) keyReleased[i]--;
                }

                if (keys[i])
                {
                    if (!keyDown[i]) keyPressed[i] = Inputbuffer;
                    keyDown[i] = true;
                }
                else
                {
                    if (keyDown[i]) keyReleased[i] = Inputbuffer;
                    keyDown[i] = false;
                }
            }

            UpdateMotionInputs(direction);
        }

        public virtual void UpdateKeys(KeyboardState keyState, GamePadState padState, MouseState mouseState, int direction)
        {
            for (int i = 0; i < keyPressed.Length; i++)
            {
                if (Game.Instance.Scene is InGame scene)
                {
                    if (scene.hitpause <= 0)
                    {
                        if (keyPressed[i] > 0) keyPressed[i]--;
                        if (keyReleased[i] > 0) keyReleased[i]--;
                    }
                }
                else
                {
                    if (keyPressed[i] > 0) keyPressed[i]--;
                    if (keyReleased[i] > 0) keyReleased[i]--;
                }

                if (id == KeyboardPort)
                {
                    if (keyState.IsKeyDown(keyBind[i]))
                    {
                        if (!keyDown[i]) keyPressed[i] = Inputbuffer;
                        keyDown[i] = true;
                    }
                    else
                    {
                        if (keyDown[i]) keyReleased[i] = Inputbuffer;
                        keyDown[i] = false;
                    }
                }

                List<Buttons> bindOverride = new List<Buttons>() { Buttons.LeftThumbstickUp, Buttons.LeftThumbstickDown, Buttons.LeftThumbstickLeft, Buttons.LeftThumbstickRight };

                if (id != KeyboardPort && GamePad.GetState(id).IsConnected)
                {
                    if (!bindOverride.Contains(CorrectedButton(padBind[i])))
                    {
                        if (padState.IsButtonDown(CorrectedButton(padBind[i])))
                        {
                            if (!keyDown[i]) keyPressed[i] = Inputbuffer;
                            keyDown[i] = true;
                        }
                        else
                        {
                            if (keyDown[i]) keyReleased[i] = Inputbuffer;
                            keyDown[i] = false;
                        }
                    }
                    if (padBind[i] == Buttons.LeftThumbstickUp)
                    {
                        if (padState.ThumbSticks.Left.Y > deadzone || padState.IsButtonDown(Buttons.DPadUp))
                        {
                            if (!keyDown[i]) keyPressed[i] = Inputbuffer;
                            keyDown[i] = true;
                        }
                        else
                        {
                            if (keyDown[i]) keyReleased[i] = Inputbuffer;
                            keyDown[i] = false;
                        }
                    }
                    if (padBind[i] == Buttons.LeftThumbstickDown)
                    {
                        if (padState.ThumbSticks.Left.Y < -deadzone || padState.IsButtonDown(Buttons.DPadDown))
                        {
                            if (!keyDown[i]) keyPressed[i] = Inputbuffer;
                            keyDown[i] = true;
                        }
                        else
                        {
                            if (keyDown[i]) keyReleased[i] = Inputbuffer;
                            keyDown[i] = false;
                        }
                    }
                    if (padBind[i] == Buttons.LeftThumbstickRight)
                    {
                        if (padState.ThumbSticks.Left.X > deadzone || padState.IsButtonDown(Buttons.DPadRight))
                        {
                            if (!keyDown[i]) keyPressed[i] = Inputbuffer;
                            keyDown[i] = true;
                        }
                        else
                        {
                            if (keyDown[i]) keyReleased[i] = Inputbuffer;
                            keyDown[i] = false;
                        }
                    }
                    if (padBind[i] == Buttons.LeftThumbstickLeft)
                    {
                        if (padState.ThumbSticks.Left.X < -deadzone || padState.IsButtonDown(Buttons.DPadLeft))
                        {
                            if (!keyDown[i]) keyPressed[i] = Inputbuffer;
                            keyDown[i] = true;
                        }
                        else
                        {
                            if (keyDown[i]) keyReleased[i] = Inputbuffer;
                            keyDown[i] = false;
                        }
                    }
                }
            }

            UpdateMotionInputs(direction);
        }

        public bool[] GetKeysDown(InputState inputs)
        {
            bool[] b = new bool[NumKeys];
            for (int i = 0; i < NumKeys; i++)
            {
                if (id == KeyboardPort)
                {
                    if (keyBind[i] < 0)
                    {
                        if (keyBind[i] == (Keys)(-1))
                        {
                            if (inputs.mouseState.LeftButton == ButtonState.Pressed)
                                b[i] = true;
                        }
                        if (keyBind[i] == (Keys)(-2))
                        {
                            if (inputs.mouseState.RightButton == ButtonState.Pressed)
                                b[i] = true;
                        }
                    }
                    else
                    {
                        if (inputs.keyState.IsKeyDown(keyBind[i]))
                            b[i] = true;
                    }
                }

                List<Buttons> bindOverride = new List<Buttons>() { Buttons.LeftThumbstickUp, Buttons.LeftThumbstickDown, Buttons.LeftThumbstickLeft, Buttons.LeftThumbstickRight, Buttons.RightThumbstickUp, Buttons.RightThumbstickDown, Buttons.RightThumbstickLeft, Buttons.RightThumbstickRight };

                if (id != KeyboardPort &&  GamePad.GetState(id).IsConnected)
                {
                    if (!bindOverride.Contains(CorrectedButton(padBind[i])))
                    {
                        if (inputs.padState.IsButtonDown(CorrectedButton(padBind[i])))
                            b[i] = true;
                    }
                    if (padBind[i] == Buttons.LeftThumbstickUp)
                    {
                        if (inputs.padState.ThumbSticks.Left.Y > deadzone || inputs.padState.IsButtonDown(Buttons.DPadUp))
                            b[i] = true;
                    }
                    if (padBind[i] == Buttons.LeftThumbstickDown)
                    {
                        if (inputs.padState.ThumbSticks.Left.Y < -deadzone || inputs.padState.IsButtonDown(Buttons.DPadDown))
                            b[i] = true;
                    }
                    if (padBind[i] == Buttons.LeftThumbstickRight)
                    {
                        if (inputs.padState.ThumbSticks.Left.X > deadzone || inputs.padState.IsButtonDown(Buttons.DPadRight))
                            b[i] = true;
                    }
                    if (padBind[i] == Buttons.LeftThumbstickLeft)
                    {
                        if (inputs.padState.ThumbSticks.Left.X < -deadzone || inputs.padState.IsButtonDown(Buttons.DPadLeft))
                            b[i] = true;
                    }
                }
            }
            return b;
        }

        protected void UpdateMotionInputs(int direction)
        {
            //Double tap to dash
            if (keyPressed[Key_Right] == Inputbuffer - 1)
            {
                if (dTapDashBufferR > 0 && dTapDashBufferR <= 10)
                {
                    keyPressed[Key_Dash] = Inputbuffer;
                    dTapDashBufferR = 0;
                }
                else dTapDashBufferR = 15;
            }

            if (dTapDashBufferR > 0) dTapDashBufferR--;

            if (keyPressed[Key_Left] == Inputbuffer - 1)
            {
                if (dTapDashBufferL > 0 && dTapDashBufferL <= 10)
                {
                    keyPressed[Key_Dash] = Inputbuffer;
                    dTapDashBufferL = 0;
                }
                else dTapDashBufferL = 15;
            }

            if (dTapDashBufferL > 0) dTapDashBufferL--;

            if (keyPressed[Key_Down] == Inputbuffer - 1)
            {
                if (dTapDown[0] > 0) dTapDown[1] = motionInputBuffer;
                dTapDown[0] = motionInputBuffer;
            }

            if (keyDown[Key_Down])
            {
                dTapDashBufferL = 0;
                dTapDashBufferR = 0;
            }
            for (int i = 0; i < 2; i++)
            {
                //Reduce buffers
                if (qcForward[i] > 0) qcForward[i]--;
                if (qcBackwards[i] > 0) qcBackwards[i]--;
                if (dTapDown[i] > 0) dTapDown[i]--;
            }
            for (int i = 0; i < 3; i++)
            {
                //Reduce buffers
                if (hcForward[i] > 0) hcForward[i]--;
                if (hcBackwards[i] > 0) hcBackwards[i]--;
                if (dpForward[i] > 0) dpForward[i]--;
                if (dpBackwards[i] > 0) dpBackwards[i]--;
            }
            if (chargeBack[1] > 0) chargeBack[1]--;
            if (chargeDown[1] > 0) chargeDown[1]--;

            //Get X direction of the stick relative to the way the player is facing
            int stickX = 0;
            if (keyDown[Key_Left]) stickX--;
            if (keyDown[Key_Right]) stickX++;
            stickX *= direction;
            //Get Y direction of the stick
            int stickY = 0;
            if (keyDown[Key_Down]) stickY++;
            if (keyDown[Key_Up]) stickY--;

            //Quarter circles
            if (stickY == 1 && stickX == 0)
            {
                qcForward[0] = motionInputBuffer;
                qcForward[1] = 0;
                qcBackwards[0] = motionInputBuffer;
                qcBackwards[1] = 0;
            }

            if (stickY == 0 && stickX == 1 && qcForward[0] > 0)
            {
                qcForward[0] = 0;
                qcForward[1] = motionInputBuffer;
            }
            if (stickY != 1 && stickX != 1) qcForward[0] = 0;

            if (stickY == 0 && stickX == -1 && qcBackwards[0] > 0)
            {
                qcBackwards[0] = 0;
                qcBackwards[1] = motionInputBuffer;
            }
            if (stickY != 1 && stickX != -1) qcBackwards[0] = 0;

            //Half circles
            if (stickX == -1 && stickY == 0) hcForward[0] = motionInputBuffer;
            if (stickX == 1 && stickY == 0) hcBackwards[0] = motionInputBuffer;
            if (stickX == 0 && stickY == 1)
            {
                if (hcForward[0] > 0)
                {
                    hcForward[0] = 0;
                    hcForward[1] = motionInputBuffer;
                }
                if (hcBackwards[0] > 0)
                {
                    hcBackwards[0] = 0;
                    hcBackwards[1] = motionInputBuffer;
                }
            }
            if (stickY == 0 && stickX == 1 && hcForward[1] > 0)
            {
                hcForward[1] = 0;
                hcForward[2] = motionInputBuffer;
            }
            if (stickY == 0 && stickX == -1 && hcBackwards[1] > 0)
            {
                hcBackwards[1] = 0;
                hcBackwards[2] = motionInputBuffer;
            }

            //Charges
            if (stickX == -1) chargeBack[0] = Math.Min(chargeBack[0] + 1, 60);
            else if (chargeBack[0] > 0) chargeBack[0]--;
            if (stickY == 1) chargeDown[0] = Math.Min(chargeDown[0] + 1, 60);
            else if (chargeDown[0] > 0) chargeDown[0]--;
            if (stickX == 1 && chargeBack[0] > 45)
            {
                chargeBack[0] = 0;
                chargeBack[1] = motionInputBuffer;
            }
            if (stickY == -1 && chargeDown[0] > 45)
            {
                chargeDown[0] = 0;
                chargeDown[1] = motionInputBuffer;
            }

            //DP Motion
            if (stickX == -1 && stickY == 0) dpBackwards[0] = motionInputBuffer;
            if (stickX == 1 && stickY == 0) dpForward[0] = motionInputBuffer;
            if (stickX == 0 && stickY == 1)
            {
                if (dpForward[0] > 0)
                {
                    dpForward[0] = 0;
                    dpForward[1] = motionInputBuffer;
                }
                if (dpBackwards[0] > 0)
                {
                    dpBackwards[0] = 0;
                    dpBackwards[1] = motionInputBuffer;
                }
            }
            if (stickY == 1 && stickX == 1 && dpForward[1] > 0)
            {
                dpForward[1] = 0;
                dpForward[2] = motionInputBuffer;
            }
            if (stickY == 1 && stickX == -1 && dpBackwards[1] > 0)
            {
                dpBackwards[1] = 0;
                dpBackwards[2] = motionInputBuffer;
            }
        }

        public void ClearBuffer(int key)
        {
            keyPressed[key] = 0;
            keyReleased[key] = 0;
        }

        public void ClearAllBuffers()
        {
            for (int i = 0; i < NumKeys; i++)
            {
                keyPressed[i] = 0;
                keyReleased[i] = 0;
            }
        }

        public void ClearAllInputs()
        {
            for (int i = 0; i < NumKeys; i++)
            {
                keyPressed[i] = 0;
                keyDown[i] = false;
                keyReleased[i] = 0;
            }
        }

        public void PressKey(int key)
        {
            keyPressed[key] = Inputbuffer;
            keyDown[key] = true;
        }

        public bool KeyPressed(int key)
        {
            return keyPressed[key] > 0 && keyPressed[key] <= Inputbuffer;
        }

        public bool KeyDown(int key)
        {
            return (keyDown[key] && keyPressed[key] <= Inputbuffer) || keyReleased[key] > Inputbuffer;
        }

        public bool KeyReleased(int key)
        {
            return keyReleased[key] > 0 && keyReleased[key] <= Inputbuffer;
        }

        public bool MotionDoubleDown { get => dTapDown[1] > 0; }
        public bool MotionQCF { get => qcForward[1] > 0; }
        public bool MotionQCB { get => qcBackwards[1] > 0; }
        public bool MotionHCF { get => hcForward[2] > 0; }
        public bool MotionHCB { get => hcBackwards[2] > 0; }
        public bool MotionCharegDown { get => chargeDown[1] > 0; }
        public bool MotionCharegBack { get => chargeBack[1] > 0; }
        public bool MotionDPF { get => dpForward[2] > 0; }
        public bool MotionDPB { get => dpBackwards[2] > 0; }

        public void RebindKeyboard(int input, Keys key)
        {
            keyBind[input] = key;
        }
        public void RebindController(int input, Buttons button)
        {
            padBind[input] = button;
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < NumKeys; i++)
            {
                bytes.Add((byte)keyPressed[i]);
                bytes.Add((byte)(keyDown[i] ? 1 : 0));
                bytes.Add((byte)keyReleased[i]);
            }

            bytes.Add((byte)dTapDashBufferR);
            bytes.Add((byte)dTapDashBufferL);

            for (int i = 0; i < 2; i++) bytes.Add((byte)dTapDown[i]);
            for (int i = 0; i < 2; i++) bytes.Add((byte)qcForward[i]);
            for (int i = 0; i < 2; i++) bytes.Add((byte)qcBackwards[i]);
            for (int i = 0; i < 3; i++) bytes.Add((byte)hcForward[i]);
            for (int i = 0; i < 3; i++) bytes.Add((byte)hcBackwards[i]);
            for (int i = 0; i < 2; i++) bytes.Add((byte)chargeDown[i]);
            for (int i = 0; i < 2; i++) bytes.Add((byte)chargeBack[i]);
            for (int i = 0; i < 3; i++) bytes.Add((byte)dpForward[i]);
            for (int i = 0; i < 3; i++) bytes.Add((byte)dpBackwards[i]);
            return bytes.ToArray();
        }

        public void ReadBytes(byte[] bytes)
        {
            int i = 0;
            for (i = 0; i < keyPressed.Length * 3; i += 3)
            {
                keyPressed[i / 3] = bytes[i];
                keyDown[i / 3] = bytes[i + 1] == 1;
                keyReleased[i / 3] = bytes[i + 2];
            }

            dTapDashBufferR = bytes[i];
            dTapDashBufferL = bytes[i + 1];

            for (int j = 0; j < 2; j++) dTapDown[j] = bytes[i + 2 + j];
            for (int j = 0; j < 2; j++) qcForward[j] = bytes[i + 4 + j];
            for (int j = 0; j < 2; j++) qcBackwards[j] = bytes[i + 6 + j];
            for (int j = 0; j < 3; j++) hcForward[j] = bytes[i + 8 + j];
            for (int j = 0; j < 3; j++) hcBackwards[j] = bytes[i + 11 + j];
            for (int j = 0; j < 2; j++) chargeDown[j] = bytes[i + 14 + j];
            for (int j = 0; j < 2; j++) chargeBack[j] = bytes[i + 16 + j];
            for (int j = 0; j < 3; j++) dpForward[j] = bytes[i + 18 + j];
            for (int j = 0; j < 3; j++) dpBackwards[j] = bytes[i + 21 + j];
        }

        public byte[] ToSimpleBytes()
        {
            byte[] bytes = new byte[NumKeys];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(keyDown[i] ? 1 : 0);
            }
            return bytes;
        }

        public void ReadSimpleBytes(byte[] bytes)
        {
            for (int i = 0; i < keyPressed.Length; i++)
            {
                if (bytes[i] == 1 && !keyDown[i]) keyPressed[i] = Inputbuffer;
                if (bytes[i] == 0 && keyDown[i]) keyReleased[i] = Inputbuffer;
                keyDown[i] = bytes[i] == 1;
            }
        }

        public static Dictionary<Buttons, Texture2D> ControllerButtonIcons = new Dictionary<Buttons, Texture2D>()
        {
            { Buttons.A, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Controller_A") },
            { Buttons.B, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Controller_B") },
            { Buttons.X, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Controller_X") },
            { Buttons.Y, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Controller_Y") },
            { Buttons.LeftShoulder, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Controller_LB") },
            { Buttons.LeftTrigger, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Controller_LT") },
            { Buttons.RightShoulder, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Controller_RB") },
            { Buttons.RightTrigger, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Controller_RT") },
            { Buttons.LeftStick, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Controller_LS") },
            { Buttons.RightStick, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Controller_RS") }
        };

        public static Dictionary<InputMotions, Texture2D> ControllerMotionIcons = new Dictionary<InputMotions, Texture2D>()
        {
            { InputMotions.None, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_Neutral") },
            { InputMotions.Forward, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_Right") },
            { InputMotions.Back, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_Left") },
            { InputMotions.Down, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_Down") },
            { InputMotions.DownDown, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_DownDown") },
            { InputMotions.DownForward, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_DownRight") },
            { InputMotions.DownBack, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_DownLeft") },
            { InputMotions.QuarterForward, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_QCRight") },
            { InputMotions.QuarterBack, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_QCLeft") },
            { InputMotions.HalfForward, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_HCRight") },
            { InputMotions.HalfBack, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_HCLeft") },
        };

        public static Dictionary<InputMotions, Texture2D> ControllerBackwardsMotionIcons = new Dictionary<InputMotions, Texture2D>()
        {
            { InputMotions.None, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_Neutral") },
            { InputMotions.Forward, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_Left") },
            { InputMotions.Back, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_Right") },
            { InputMotions.Down, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_Down") },
            { InputMotions.DownDown, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_DownDown") },
            { InputMotions.DownForward, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_DownLeft") },
            { InputMotions.DownBack, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_DownRight") },
            { InputMotions.QuarterForward, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_QCLeft") },
            { InputMotions.QuarterBack, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_QCRight") },
            { InputMotions.HalfForward, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_HCLeft") },
            { InputMotions.HalfBack, Game.LoadAsset<Texture2D>("UI/ButtonIcons/Motion_HCRight") },
        };
    }

    public struct InputState
    {
        public KeyboardState keyState;
        public GamePadState padState;
        public MouseState mouseState;

        public InputState(int gamePadID)
        {
            this.keyState = Keyboard.GetState();
            this.padState = GamePad.GetState(gamePadID);
            this.mouseState = Mouse.GetState();
        }

        public InputState(InputState other)
        {
            this.keyState = other.keyState;
            this.padState = other.padState;
            this.mouseState = other.mouseState;
        }

        public InputState(KeyboardState keyState, GamePadState padState, MouseState mouseState)
        {
            this.keyState = keyState;
            this.padState = padState;
            this.mouseState = mouseState;
        }
    }

    public class ControlProfile
    {
        public string name = "Default";

        public Keys[] keyBinds =
            [
                Keys.W,
                Keys.S,
                Keys.A,
                Keys.D,
                Keys.J,
                Keys.K,
                Keys.L,
                Keys.U,
                Keys.I,
            ];
        public Buttons[] padBinds =
            [
                Buttons.X,
                Buttons.A,
                Buttons.B,
                Buttons.Y,
                Buttons.LeftTrigger,
            ];

        public ControlProfile() { }

        public static ControlProfile LoadFromBytes(byte[] data, string name)
        {
            ControlProfile c = new ControlProfile();
            c.name = name;
            int pos = 3;

            //Key Bindings
            int i = 0;
            while (pos < data.Length && i < c.keyBinds.Length && data[pos] != '|')
            {
                c.keyBinds[i] = (Keys)data[pos];
                pos++;
                i++;
            }
            if (pos < data.Length && data[pos] == '|') pos++;
            else return c;

            //Pad Bindings
            i = 0;
            while (pos < data.Length && i < c.padBinds.Length && data[pos] != '}')
            {
                int num = BitConverter.ToInt32(data, pos);
                c.padBinds[i] = (Buttons)num;
                pos += 4;
                i++;
            }

            return c;
        }

        public byte[] ToBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(0);
            bytes.Add(0);
            bytes.Add((byte)'|');

            foreach (Keys k in keyBinds) bytes.Add((byte)k);
            bytes.Add((byte)'|');
            foreach (Buttons b in padBinds)
            {
                bytes.Add(BitConverter.GetBytes((int)b)[0]);
                bytes.Add(BitConverter.GetBytes((int)b)[1]);
                bytes.Add(BitConverter.GetBytes((int)b)[2]);
                bytes.Add(BitConverter.GetBytes((int)b)[3]);
            }

            return bytes.ToArray();
        }
    }
}
