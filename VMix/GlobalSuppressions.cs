// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Collections are bbindable properties which need to be set.", Scope = "member", Target = "~P:VMix.ViewModel.SelectedItemsViewModel.SelectedChannels")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>", Scope = "member", Target = "~M:VMix.MidiManager.OpenMidiDevice(Commons.Music.Midi.IMidiPortDetails,Commons.Music.Midi.IMidiPortDetails)")]
[assembly: SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>", Scope = "member", Target = "~M:VMix.MidiManager.HandleSendFaderMsg(System.Object,System.ComponentModel.PropertyChangedEventArgs,VMix.Channel)")]
