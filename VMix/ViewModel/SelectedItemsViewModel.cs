using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace VMix.ViewModel
{
    /// <summary>
    /// This class stores a list of references to all currently selected channels.
    /// It exposes the same properties as a MixChannel which can be bound to directly and will be bound to the corresponding properties in the MixChannel
    /// </summary>
    public class SelectedItemsViewModel : MixChannel
    {
        #region Selected channels collection and notifier
        private ObservableCollection<MixChannel> selectedChannels = new ObservableCollection<MixChannel>();
        private Dictionary<string, List<Parameter>> selectedChannelsParametersFlattened = new Dictionary<string, List<Parameter>>();
        private Dictionary<string, Parameter> localParametersFlattened = new Dictionary<string, Parameter>();

        private bool dontPropagateToSelection = false;

        public ObservableCollection<MixChannel> SelectedChannels
        {
            get { return selectedChannels; }
            set { SetProperty(ref selectedChannels, value); }
        }

        public SelectedItemsViewModel(MixChannel template, int index = 0) : base(index)
        {
            //Fill in our arrays so that they have enough elements using the template if it exists
            //This is lazy but might work
            if (template != null)
            {
                foreach (PropertyInfo p in template.GetType().GetProperties())
                {
                    if (typeof(INotifyCollectionChanged).IsAssignableFrom(p.PropertyType))
                    {
                        p.SetValue(this, p.GetValue(template));
                    }
                }
            }

            //Now we add change listners to our virtual parameters such that when modified by the GUI we can reflect those changes in the selection
            AddLocalPropertyChangeEventHandlers(this);
            LocalParameterDictionaryBuilder(this);

            //Whenever we change selection add and remove notifiers from the selected channels we are listning to for changes.
            //Additionally, we need to update our flattened reference dictionary of addressable references to the properties of the selection.
            SelectedChannels = new ObservableCollection<MixChannel>();
            SelectedChannels.CollectionChanged += (o, e) =>
            {
                //We need to listen for changes in the model (if the physical mixer hardware changes model values)
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    // Subscribe each to PropertyChanged, using Item_PropertyChanged
                    if (e.NewItems == null)
                        return;

                    foreach (MixChannel m in e.NewItems)
                    {
                        //Rebuild refs
                        RemoteParameterDictionaryBuilder(m);
                        //Add new change listners
                        AddRemotePropertyChangedEventHandlers(m);
                    }

                    //Even though we are about to add property change delegates we still need to invoke the notification on all properties here to refelect the existing values of the new selection
                    dontPropagateToSelection = true;
                    InvokeAllLocalPropertyChangedEvents(this);
                    dontPropagateToSelection = false;
                }
                if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset)
                {
                    //Rebuild reference dictionary
                    selectedChannelsParametersFlattened.Clear();
                    foreach (MixChannel m in selectedChannels)
                        RemoteParameterDictionaryBuilder(m);

                    //TODO: This is always null which leaves unselected channels with orphaned change notifiers
                    if (e.OldItems != null)
                    {
                        foreach (MixChannel m in e.OldItems)
                            RemoveRemotePropertyChangedEventHandlers(m);
                    }

                    foreach (KeyValuePair<string, List<Parameter>> props in selectedChannelsParametersFlattened)
                        foreach (Parameter p in props.Value)
                            p.PropertyChanged -= (sender, a) => PropagatePropertyChangeToViewModel(sender, a, props.Key);//Probs won't work

                    dontPropagateToSelection = true;
                    InvokeAllLocalPropertyChangedEvents(this);
                    dontPropagateToSelection = false;
                }
            };
        }

        #endregion
        #region Property change handlers and generators

        /// <summary>
        /// Add listners to all the properties in the slection to track their changes.
        /// </summary>
        /// <param name="o"></param>
        private void AddRemotePropertyChangedEventHandlers(object o, string parentAddr = "")
        {
            //Add an event handler to this object since this is called recursively, it's children will also get handlers
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(o.GetType()))
            {
                //Console.WriteLine("[->Selection] Added property change notification handler to remote: " + parentAddr);
                //if (!typeof(INotifyCollectionChanged).IsAssignableFrom(o.GetType()))
                if (o.GetType().IsSubclassOf(typeof(Parameter)))
                    ((INotifyPropertyChanged)o).PropertyChanged += (sender, e) => PropagatePropertyChangeToViewModel(sender, e, parentAddr);
                //else
                //    ((INotifyCollectionChanged)o).CollectionChanged += (sender, e) => PropagateCollectionChangeToSelectedChannels(sender, e);
            }
            //Find all the properties which have bindable properties
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                if (parentAddr == string.Empty && p.DeclaringType != typeof(MixChannel))
                    continue;

                if (typeof(INotifyPropertyChanged).IsAssignableFrom(p.PropertyType))
                {
                    if (typeof(System.Collections.ICollection).IsAssignableFrom(o.GetType()))
                    {
                        int i = 0;
                        //If the property is a collection, add event handlers to each of it's elements
                        foreach (object oi in (System.Collections.ICollection)o)
                        {
                            if (((INotifyPropertyChanged)oi) == null)
                                continue;

                            string newAddr = (parentAddr == "" ? "" : ($"{parentAddr}[{i}]"));

                            //Recursively add more handlers
                            AddRemotePropertyChangedEventHandlers(oi, newAddr);
                            i++;
                        }
                    }
                    else
                    {
                        object propVal = p.GetValue(o);
                        //Add event listners
                        if (((INotifyPropertyChanged)propVal) == null)
                            continue;

                        string newAddr = (parentAddr == "" ? "" : (parentAddr + ".")) + p.Name;

                        //Recursively add more handlers
                        AddRemotePropertyChangedEventHandlers(propVal, newAddr);
                    }
                }
            }
        }

        /// <summary>
        /// Remove listners from all the properties in the slection to track their changes.
        /// </summary>
        /// <param name="o"></param>
        private void RemoveRemotePropertyChangedEventHandlers(object o, string parentAddr = "")
        {
            //Remove an event handler to this object since this is called recursively, it's children will also get handlers
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(o.GetType()))
            {
                //Console.WriteLine("[->Selection] Removed property change notification handler from remote: " + o);
                if (o.GetType().IsSubclassOf(typeof(Parameter)))
                    ((INotifyPropertyChanged)o).PropertyChanged -= (sender, e) => PropagatePropertyChangeToViewModel(sender, e, parentAddr);
                //else
                //    ((INotifyCollectionChanged)o).CollectionChanged -= (sender, e) => PropagateCollectionChangeToSelectedChannels(sender, e);
            }
            //Find all the properties which have bindable properties
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                if (parentAddr == string.Empty && p.DeclaringType != typeof(MixChannel))
                    continue;

                if (typeof(INotifyPropertyChanged).IsAssignableFrom(p.PropertyType))
                {
                    if (typeof(System.Collections.ICollection).IsAssignableFrom(o.GetType()))
                    {
                        int i = 0;
                        //If the property is a collection, remove event handlers to each of it's elements
                        foreach (object oi in (System.Collections.ICollection)o)
                        {
                            if (((INotifyPropertyChanged)oi) == null)
                                continue;

                            string newAddr = (parentAddr == "" ? "" : ($"{parentAddr}[{i}]"));

                            //Recursively add more handlers
                            RemoveRemotePropertyChangedEventHandlers(oi, newAddr);
                            i++;
                        }
                    }
                    else
                    {
                        object propVal = p.GetValue(o);
                        //Remove event listners
                        if (((INotifyPropertyChanged)propVal) == null)
                            continue;

                        string newAddr = (parentAddr == "" ? "" : (parentAddr + ".")) + p.Name;

                        //Recursively remove more handlers
                        RemoveRemotePropertyChangedEventHandlers(propVal, newAddr);
                    }
                }
            }
        }

        /// <summary>
        /// Build a dictionary of references to all the properties of thre selected items.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="parentAddr"></param>
        private void RemoteParameterDictionaryBuilder(object o, string parentAddr = "")
        {
            //Console.WriteLine($"[->VM]Building remote parameter dictionary for: {o} at level: {parentAddr}");
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                if (parentAddr == string.Empty && p.DeclaringType != typeof(MixChannel))
                    continue;

                if (typeof(INotifyPropertyChanged).IsAssignableFrom(p.PropertyType))
                {
                    if (typeof(System.Collections.ICollection).IsAssignableFrom(o.GetType()))
                    {
                        int i = 0;
                        foreach (object oi in (System.Collections.ICollection)o)
                        {
                            if (((INotifyPropertyChanged)oi) == null)
                                continue;

                            //Add to dictionary
                            string newAddr = (parentAddr == "" ? "" : ($"{parentAddr}[{i}]"));

                            //Recursively add more handlers
                            RemoteParameterDictionaryBuilder(oi, newAddr);
                            i++;
                        }
                    }
                    else
                    {
                        object propVal = p.GetValue(o);
                        if (((INotifyPropertyChanged)propVal) == null)
                            continue;

                        //Add to dictionary
                        string newAddr = (parentAddr == "" ? "" : (parentAddr + ".")) + p.Name;
                        if (propVal.GetType().IsSubclassOf(typeof(Parameter)))
                        {
                            if (selectedChannelsParametersFlattened.ContainsKey(newAddr))
                                selectedChannelsParametersFlattened[newAddr].Add(propVal as Parameter);
                            else
                                selectedChannelsParametersFlattened.Add(newAddr, new List<Parameter>() { (Parameter)propVal });
                        }
                        //Recursively add more handlers
                        RemoteParameterDictionaryBuilder(propVal, newAddr);
                    }
                }
            }
        }

        private void LocalParameterDictionaryBuilder(object o, string parentAddr = "")
        {
            //Console.WriteLine($"[->VM]Building local parameter dictionary for: {o} at level: {parentAddr}");
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                if (parentAddr == string.Empty && p.DeclaringType != typeof(MixChannel))
                    continue;

                if (typeof(INotifyPropertyChanged).IsAssignableFrom(p.PropertyType))
                {
                    if (typeof(System.Collections.ICollection).IsAssignableFrom(o.GetType()))
                    {
                        int i = 0;
                        foreach (object oi in (System.Collections.ICollection)o)
                        {
                            if (((INotifyPropertyChanged)oi) == null)
                                continue;

                            //Add to dictionary
                            string newAddr = (parentAddr == "" ? "" : ($"{parentAddr}[{i}]"));

                            //Recursively add more handlers
                            LocalParameterDictionaryBuilder(oi, newAddr);
                            i++;
                        }
                    }
                    else
                    {
                        object propVal = p.GetValue(o);
                        if (((INotifyPropertyChanged)propVal) == null)
                            continue;

                        //Add to dictionary
                        string newAddr = (parentAddr == "" ? "" : (parentAddr + ".")) + p.Name;
                        if (propVal.GetType().IsSubclassOf(typeof(Parameter)))
                        {
                            if (!localParametersFlattened.ContainsKey(newAddr))
                                localParametersFlattened.Add(newAddr, (Parameter)propVal);
                        }
                        //Recursively add more handlers
                        LocalParameterDictionaryBuilder(propVal, newAddr);
                    }
                }
            }
        }

        /// <summary>
        /// Invokes all the local property change listners.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="parentAddr"></param>
        private void InvokeAllLocalPropertyChangedEvents(object o, string parentAddr = "")
        {
            //Add an event handler to this object since this is called recursively, it's children will also get handlers
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(o.GetType()))
            {
                //Console.WriteLine("[->VM] Invoking local property changed event: " + parentAddr);
                if (o.GetType().IsSubclassOf(typeof(Parameter)))
                    PropagatePropertyChangeToViewModel(o, new PropertyChangedEventArgs("#SelectionChange"), parentAddr);
                //else
                //    ((INotifyPropertyChanged)o).PropertyChanged += (sender, e) => PropagatePropertyChangeToSelectedChannels(sender, e, parentAddr);
            }
            //Find all the properties which have bindable properties
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                if (parentAddr == string.Empty && p.DeclaringType != typeof(MixChannel))
                    continue;

                if (typeof(INotifyPropertyChanged).IsAssignableFrom(p.PropertyType))
                {
                    if (typeof(System.Collections.ICollection).IsAssignableFrom(o.GetType()))
                    {
                        //If the property is a collection, add event handlers to each of it's elements
                        int i = 0;
                        foreach (object oi in (System.Collections.ICollection)o)
                        {
                            if (((INotifyPropertyChanged)oi) == null)
                                continue;

                            string newAddr = (parentAddr == "" ? "" : ($"{parentAddr}[{i}]"));

                            //Recursively add more handlers
                            InvokeAllLocalPropertyChangedEvents(oi, newAddr);
                            i++;
                        }
                    }
                    else
                    {
                        object propVal = p.GetValue(o);
                        //Add event listners
                        if (((INotifyPropertyChanged)propVal) == null)
                            continue;

                        string newAddr = (parentAddr == "" ? "" : (parentAddr + ".")) + p.Name;

                        //Recursively add more handlers
                        InvokeAllLocalPropertyChangedEvents(propVal, newAddr);
                    }
                }
            }
        }

        /// <summary>
        /// Add listners to the view model to listen to changes in the view mdoel from the UI.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="parentAddr"></param>
        private void AddLocalPropertyChangeEventHandlers(object o, string parentAddr = "")
        {
            //Add an event handler to this object since this is called recursively, it's children will also get handlers
            //if (typeof(INotifyPropertyChanged).IsAssignableFrom(o.GetType()))
            if (o.GetType().IsSubclassOf(typeof(Parameter)))
            {
                //Console.WriteLine("[->VM] Added property change notification handler to local: " + parentAddr);
                if (typeof(INotifyCollectionChanged).IsAssignableFrom(o.GetType()))
                    ((INotifyCollectionChanged)o).CollectionChanged += (sender, e) => PropagateCollectionChangeToSelectedChannels(sender, e, parentAddr);
                else
                    ((INotifyPropertyChanged)o).PropertyChanged += (sender, e) => PropagatePropertyChangeToSelectedChannels(sender, e, parentAddr);
            }
            //Find all the properties which have bindable properties
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                if (parentAddr == string.Empty && p.DeclaringType != typeof(MixChannel))
                    continue;

                if (typeof(INotifyPropertyChanged).IsAssignableFrom(p.PropertyType))
                {
                    if (typeof(System.Collections.ICollection).IsAssignableFrom(o.GetType()))
                    {
                        //If the property is a collection, add event handlers to each of it's elements
                        int i = 0;
                        foreach (object oi in (System.Collections.ICollection)o)
                        {
                            if (((INotifyPropertyChanged)oi) == null)
                                continue;

                            string newAddr = (parentAddr == "" ? "" : ($"{parentAddr}[{i}]"));

                            //Recursively add more handlers
                            AddLocalPropertyChangeEventHandlers(oi, newAddr);
                            i++;
                        }
                    }
                    else
                    {
                        object propVal = p.GetValue(o);
                        //Add event listners
                        if (((INotifyPropertyChanged)propVal) == null)
                            continue;

                        string newAddr = (parentAddr == "" ? "" : (parentAddr + ".")) + p.Name;

                        //Recursively add more handlers
                        AddLocalPropertyChangeEventHandlers(propVal, newAddr);
                    }
                }
            }
        }

        /// <summary>
        /// Remove listners from the view model to listen to changes in the view mdoel from the UI.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="parentAddr"></param>
        private void RemoveLocalPropertyChangeEventHandlers(object o, string parentAddr = "")
        {
            //Add an event handler to this object since this is called recursively, it's children will also get handlers
            if (typeof(INotifyPropertyChanged).IsAssignableFrom(o.GetType()))
            {
                //Console.WriteLine("[->VM] Removed property change notification handler from local: " + parentAddr);
                if (typeof(INotifyCollectionChanged).IsAssignableFrom(o.GetType()))
                    ((INotifyCollectionChanged)o).CollectionChanged -= (sender, e) => PropagateCollectionChangeToSelectedChannels(sender, e, parentAddr);
                else
                    ((INotifyPropertyChanged)o).PropertyChanged -= (sender, e) => PropagatePropertyChangeToSelectedChannels(sender, e, parentAddr);
            }
            //Find all the properties which have bindable properties
            foreach (PropertyInfo p in o.GetType().GetProperties())
            {
                if (parentAddr == string.Empty && p.DeclaringType != typeof(MixChannel))
                    continue;

                if (typeof(INotifyPropertyChanged).IsAssignableFrom(p.PropertyType))
                {
                    if (typeof(System.Collections.ICollection).IsAssignableFrom(o.GetType()))
                    {
                        //If the property is a collection, add event handlers to each of it's elements
                        int i = 0;
                        foreach (object oi in (System.Collections.ICollection)o)
                        {
                            if (((INotifyPropertyChanged)oi) == null)
                                continue;

                            string newAddr = (parentAddr == "" ? "" : ($"{parentAddr}[{i}]"));

                            //Recursively add more handlers
                            RemoveLocalPropertyChangeEventHandlers(oi, newAddr);
                            i++;
                        }
                    }
                    else
                    {
                        object propVal = p.GetValue(o);
                        //Add event listners
                        if (((INotifyPropertyChanged)propVal) == null)
                            continue;

                        string newAddr = (parentAddr == "" ? "" : (parentAddr + ".")) + p.Name;

                        //Recursively add more handlers
                        RemoveLocalPropertyChangeEventHandlers(propVal, newAddr);
                    }
                }
            }
        }

        private void PropagateCollectionChangeToSelectedChannels(object sender, NotifyCollectionChangedEventArgs e, string addr)
        {
            Console.WriteLine("[->Selection][Unimplemented] Propagating collection change from: " + addr + " with action: " + e.Action + " to selected channels.");
        }

        /// <summary>
        /// When a property on our view mdoel changes, propagate it to the selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="addr"></param>
        private void PropagatePropertyChangeToSelectedChannels(object sender, PropertyChangedEventArgs e, string addr)
        {
            //This is a special case where we don't want to propaget changes to prevent two-way bindings fighting
            if (dontPropagateToSelection || addr == string.Empty)
                return;

            //Console.WriteLine("[->Selection] Propagating property change from: " + addr + " with property: " + e.PropertyName + " to selected channels.");

            if (SelectedChannels.Count == 0)
                return;

            List<Parameter> selectionParams = selectedChannelsParametersFlattened[addr];
            Parameter vmParam = localParametersFlattened[addr];
            vmParam.MultipleValues = false;
            for (int i = 0; i < selectionParams.Count; i++)
                selectionParams[i].CopyValueFrom(vmParam);//This breaks referencial integrity
        }

        /// <summary>
        /// When a property on an item in our selection changes, propagate it to the view model.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <param name="addr"></param>
        private void PropagatePropertyChangeToViewModel(object sender, PropertyChangedEventArgs e, string addr)
        {
            if (!localParametersFlattened.ContainsKey(addr))
                return;
            //Console.WriteLine("[->VM] Propagating property change from: " + addr + " with property: " + e.PropertyName + " to view model.");

            //Update VM from selection
            if (SelectedChannels.Count == 0)
            {
                //No selection
                Type targetParamType = localParametersFlattened[addr].GetType();
                localParametersFlattened[addr].CopyValueFrom((Parameter)Activator.CreateInstance(targetParamType));
            }
            else if (selectedChannelsParametersFlattened[addr].Distinct().Count() == 1)
            {
                //Selection with all identical values
                localParametersFlattened[addr].CopyValueFrom(selectedChannelsParametersFlattened[addr][0]);
            }
            else
            {
                //Selection with different values
                Parameter targetParam = (Parameter)Activator.CreateInstance(localParametersFlattened[addr].GetType());
                targetParam.MultipleValues = true;
                localParametersFlattened[addr].CopyValueFrom(targetParam);
            }

            //In case the value of a property in our selected items changes, 
            //notify our local version of the property that it has changed so that the binding can update
            OnPropertyChanged(e.PropertyName);
            localParametersFlattened[addr].OnPropertyChanged("Value");
        }
        #endregion
    }
}
