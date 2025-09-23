// Copyright (c) 2025 Koji Hasegawa.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

namespace FileServer
{
    public class FileServerWindow : MonoBehaviour
    {
        private Dropdown _ipAddressDropdown;
        private InputField _portInputField;
        private Toggle _openBrowserToggle;
        private Button _connectButton;

        private FileServer _fileServer = new FileServer();

        private void Start()
        {
            _ipAddressDropdown = GetComponentInChildren<Dropdown>();
            _ipAddressDropdown.ClearOptions();
            _ipAddressDropdown.AddOptions(GetLocalIPv4Addresses());

            _portInputField = GetComponentInChildren<InputField>();
            _portInputField.text = "9696";

            _openBrowserToggle = GetComponentInChildren<Toggle>();
            _openBrowserToggle.isOn = true;

            _connectButton = GetComponentInChildren<Button>();
            _connectButton.onClick.AddListener(OnConnectButtonClicked);
            ChangeState();
        }

        private void OnConnectButtonClicked()
        {
            if (_fileServer.IsRunning)
            {
                _fileServer.StopServer();
            }
            else
            {
                _fileServer.IPAddress = _ipAddressDropdown.options[_ipAddressDropdown.value].text;
                _fileServer.Port = _portInputField.text;
                _fileServer.OpenBrowser = _openBrowserToggle.isOn;
                _fileServer.StartServer();
            }

            ChangeState();
        }

        private void ChangeState()
        {
            if (_fileServer.IsRunning)
            {
                _ipAddressDropdown.interactable = false;
                _portInputField.interactable = false;
                _openBrowserToggle.interactable = false;
                _connectButton.GetComponentInChildren<Text>().text = "Stop Server";
            }
            else
            {
                _ipAddressDropdown.interactable = true;
                _portInputField.interactable = true;
                _openBrowserToggle.interactable = true;
                _connectButton.GetComponentInChildren<Text>().text = "Start Server";
            }
        }

        private static List<Dropdown.OptionData> GetLocalIPv4Addresses()
        {
            var options = new List<Dropdown.OptionData>();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;
                options.AddRange(
                    ni.GetIPProperties().UnicastAddresses
                        .Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        .Select(ip => new Dropdown.OptionData(ip.Address.ToString()))
                );
            }

            return options;
        }
    }
}
