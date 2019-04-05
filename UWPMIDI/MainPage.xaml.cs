using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.Devices.Enumeration;
using Windows.Devices.Midi;
using System.Threading.Tasks;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWPMIDI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        MidiInPort midiInPort;
        IMidiOutPort midiOutPort;

        MidiDeviceWatcher inputDeviceWatcher;
        MidiDeviceWatcher outputDeviceWatcher;

        public MainPage()
        {
            this.InitializeComponent();

            inputDeviceWatcher =
                new MidiDeviceWatcher(MidiInPort.GetDeviceSelector(), midiInPortListBox, Dispatcher);

            inputDeviceWatcher.StartWatcher();

            outputDeviceWatcher =
                new MidiDeviceWatcher(MidiOutPort.GetDeviceSelector(), midiOutPortListBox, Dispatcher);

            outputDeviceWatcher.StartWatcher();            

            byte channel = 0;
            byte note = 60;
            byte velocity = 127;
            IMidiMessage midiMessageToSend = new MidiNoteOnMessage(channel, note, velocity);
        }

        private async Task EnumerateMidiInputDevices()
        {
            // Find all input MIDI devices
            string midiInputQueryString = MidiInPort.GetDeviceSelector();
            DeviceInformationCollection midiInputDevices = await DeviceInformation.FindAllAsync(midiInputQueryString);

            midiInPortListBox.Items.Clear();

            // Return if no external devices are connected
            if (midiInputDevices.Count == 0)
            {
                this.midiInPortListBox.Items.Add("No MIDI input devices found!");
                this.midiInPortListBox.IsEnabled = false;
                return;
            }

            // Else, add each connected input device to the list
            foreach (DeviceInformation deviceInfo in midiInputDevices)
            {
                this.midiInPortListBox.Items.Add(deviceInfo.Name);
            }
            this.midiInPortListBox.IsEnabled = true;
        }

        private async Task EnumerateMidiOutputDevices()
        {

            // Find all output MIDI devices
            string midiOutportQueryString = MidiOutPort.GetDeviceSelector();
            DeviceInformationCollection midiOutputDevices = await DeviceInformation.FindAllAsync(midiOutportQueryString);

            midiOutPortListBox.Items.Clear();

            // Return if no external devices are connected
            if (midiOutputDevices.Count == 0)
            {
                this.midiOutPortListBox.Items.Add("No MIDI output devices found!");
                this.midiOutPortListBox.IsEnabled = false;
                return;
            }

            // Else, add each connected input device to the list
            foreach (DeviceInformation deviceInfo in midiOutputDevices)
            {
                this.midiOutPortListBox.Items.Add(deviceInfo.Name);
            }
            this.midiOutPortListBox.IsEnabled = true;
        }

        private async void midiInPortListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var deviceInformationCollection = inputDeviceWatcher.DeviceInformationCollection;

            if (deviceInformationCollection == null)
            {
                return;
            }

            DeviceInformation devInfo = deviceInformationCollection.Count > 0 ? 
                deviceInformationCollection[midiInPortListBox.SelectedIndex] : null;

            if (devInfo == null)
            {
                return;
            }

            midiInPort = await MidiInPort.FromIdAsync(devInfo.Id);

            if (midiInPort == null)
            {
                System.Diagnostics.Debug.WriteLine("Unable to create MidiInPort from input device");
                return;
            }
            midiInPort.MessageReceived += MidiInPort_MessageReceived;
        }

        private void MidiInPort_MessageReceived(MidiInPort sender, MidiMessageReceivedEventArgs args)
        {
            IMidiMessage receivedMidiMessage = args.Message;

            System.Diagnostics.Debug.WriteLine(receivedMidiMessage.Timestamp.ToString());

            if (receivedMidiMessage.Type == MidiMessageType.NoteOn)
            {
                byte channel = ((MidiNoteOnMessage)receivedMidiMessage).Channel;
                byte note = ((MidiNoteOnMessage)receivedMidiMessage).Note;
                byte velocity = ((MidiNoteOnMessage)receivedMidiMessage).Velocity;
                int octave = note / 12;
                int fundamental = note % 12;
                System.Diagnostics.Debug.WriteLine(channel);
                System.Diagnostics.Debug.WriteLine(note);
                System.Diagnostics.Debug.WriteLine(velocity);
                System.Diagnostics.Debug.WriteLine(octave);
                System.Diagnostics.Debug.WriteLine(fundamental);
      

                IMidiMessage message = new MidiNoteOnMessage(channel, note, velocity);
                midiOutPort.SendMessage(message);
            }
            else if(receivedMidiMessage.Type == MidiMessageType.NoteOff)
            {
                byte channel = ((MidiNoteOffMessage)receivedMidiMessage).Channel;
                byte note = ((MidiNoteOffMessage)receivedMidiMessage).Note;
                byte velocity = ((MidiNoteOffMessage)receivedMidiMessage).Velocity;
                IMidiMessage message = new MidiNoteOffMessage(channel, note, velocity);
                midiOutPort.SendMessage(message);
            }
        }

        private async void midiOutPortListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var deviceInformationCollection = outputDeviceWatcher.DeviceInformationCollection;

            if (deviceInformationCollection == null)
            {
                return;
            }

            DeviceInformation devInfo = deviceInformationCollection[midiOutPortListBox.SelectedIndex];

            if (devInfo == null)
            {
                return;
            }

            midiOutPort = await MidiOutPort.FromIdAsync(devInfo.Id);

            if (midiOutPort == null)
            {
                System.Diagnostics.Debug.WriteLine("Unable to create MidiOutPort from output device");
                return;
            }

            else
            {
                byte channel = 0;
                byte note = 60;
                byte velocity = 127;
                IMidiMessage midiMessageToSend = new MidiNoteOnMessage(channel, note, velocity);

                midiOutPort.SendMessage(midiMessageToSend);
            }

        }
    }
}
