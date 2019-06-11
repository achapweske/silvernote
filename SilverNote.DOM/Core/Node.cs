/*
 * Copyright (c) Adam Chapweske
 * 
 * Licensed under MIT (https://github.com/achapweske/silvernote/blob/master/LICENSE)
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;

namespace DOM
{
    /// <summary>
    /// A node in the document tree.
    /// </summary>
    /// <remarks>
    /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-1950641247
    /// </remarks>
    public interface Node
    {
        /// <summary>
        /// Get the Document to which this node belongs.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#node-ownerDoc:
        /// "The Document object associated with this node. This is also the 
        /// Document object used to create new nodes. When this node is a 
        /// Document or a DocumentType which is not used with any Document yet, 
        /// this is null."
        /// </remarks>
        Document OwnerDocument { get; }

        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-111237558:
        /// "A code representing the type of the underlying object..."
        /// 
        ///     Type                    NodeType
        ///     ----------              ----------
        ///     Attr                    ATTRIBUTE_NODE
        ///     CDATASection            CDATA_SECTION_NODE
        ///     Comment                 COMMENT_NODE
        ///     DocumentFragment        DOCUMENT_FRAGMENT_NODE
        ///     Document                DOCUMENT_NODE
        ///     DocumentType            DOCUMENT_TYPE_NODE
        ///     Element                 ELEMENT_NODE
        ///     Entity                  ENTITY_NODE
        ///     EntityReference         ENTITY_REFERENCE_NODE
        ///     Notation                NOTATION_NODE
        ///     ProcessingInstruction   PROCESSING_INSTRUCTION_NODE
        ///     Text                    TEXT_NODE
        ///     
        /// </remarks>
        NodeType NodeType { get; }

        /// <summary>
        /// Get this node's namespace URI (null if none)
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-NodeNSname:
        /// "The namespace URI of this node, or null if it is unspecified (see 
        /// XML Namespaces).
        /// 
        /// This is not a computed value that is the result of a namespace 
        /// lookup based on an examination of the namespace declarations in 
        /// scope. It is merely the namespace URI given at creation time.
        /// For nodes of any type other than ELEMENT_NODE and ATTRIBUTE_NODE 
        /// and nodes created with a DOM Level 1 method, such as 
        /// Document.createElement(), this is always null.
        /// 
        /// Note: Per the Namespaces in XML Specification [XML Namespaces] an 
        /// attribute does not inherit its namespace from the element it is 
        /// attached to. If an attribute is not explicitly given a namespace, 
        /// it simply has no namespace."
        /// </remarks>
        string NamespaceURI { get; }

        /// <summary>
        /// Get this node's namespace prefix (null if none)
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-NodeNSPrefix:
        /// "The namespace prefix of this node, or null if it is unspecified. 
        /// When it is defined to be null, setting it has no effect, including 
        /// if the node is read-only. Note that setting this attribute, when 
        /// permitted, changes the nodeName attribute, which holds the 
        /// qualified name, as well as the tagName and name attributes of the 
        /// Element and Attr interfaces, when applicable. Setting the prefix to 
        /// null makes it unspecified, setting it to an empty string is 
        /// implementation dependent.
        /// 
        /// Note also that changing the prefix of an attribute that is known to 
        /// have a default value, does not make a new attribute with the
        /// default value and the original prefix appear, since the 
        /// namespaceURI and localName do not change.
        /// 
        /// For nodes of any type other than ELEMENT_NODE and ATTRIBUTE_NODE and 
        /// nodes created with a DOM Level 1 method, such as createElement from 
        /// the Document interface, this is always null.
        /// 
        /// Exceptions on setting:
        /// 
        ///   INVALID_CHARACTER_ERR: Raised if the specified prefix contains an 
        ///   illegal character according to the XML version in use specified 
        ///   in the Document.xmlVersion attribute.
        /// 
        ///   NO_MODIFICATION_ALLOWED_ERR: Raised if this node is readonly.
        /// 
        ///   NAMESPACE_ERR: Raised if the specified prefix is malformed per 
        ///   the Namespaces in XML specification, if the namespaceURI of this 
        ///   node is null, if the specified prefix is "xml" and the 
        ///   namespaceURI of this node is different from 
        ///   "http://www.w3.org/XML/1998/namespace", if this node is an 
        ///   attribute and the specified prefix is "xmlns" and the 
        ///   namespaceURI of this node is different from 
        ///   "http://www.w3.org/2000/xmlns/", or if this node is an attribute 
        ///   and the qualifiedName of this node is "xmlns" [XML Namespaces]."
        ///   
        /// </remarks>
        string Prefix { get; set; }

        /// <summary>
        /// Get this node's qualified name (i.e. the node name INCLUDING any prefix)
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-F68D095:
        /// "The name of this node, depending on its type..."
        /// 
        ///     Type                    NodeName
        ///     ----------              ----------
        ///     Attr                    same as Attr.name
        ///     CDATASection            "#cdata-section"
        ///     Comment                 "#comment"
        ///     DocumentFragment        "#document-fragment"
        ///     Document                "#document"
        ///     DocumentType            same as DocumentType.name
        ///     Element                 same as Element.tagName
        ///     Entity                  entity name
        ///     EntityReference         name of entity referenced
        ///     Notation                notation name
        ///     ProcessingInstruction   same as ProcessingInstruction.target
        ///     Text                    "#text"
        /// 
        /// </remarks>
        string NodeName { get; }

        /// <summary>
        /// Get this node's local name (i.e. the node name NOT including any prefix).
        /// 
        /// This is null if the node does not have a namespace URI.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-NodeNSLocalN:
        /// "Returns the local part of the qualified name of this node.
        /// 
        /// For nodes of any type other than ELEMENT_NODE and ATTRIBUTE_NODE
        /// and nodes created with a DOM Level 1 method, such as
        /// Document.createElement(), this is always null."
        /// </remarks>
        string LocalName { get; }

        /// <summary>
        /// Get the namespace URI associated with the given prefix.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#Node3-lookupNamespaceURI:
        /// "Look up the namespace URI associated to the given prefix, 
        /// starting from this node.
        /// 
        /// See Namespace URI Lookup for details on the algorithm used by this method."
        /// </remarks>
        /// <param name="prefix">The prefix to look for. If this parameter is 
        /// null, the method will return the default namespace URI if any.</param>
        /// <returns>the associated namespace URI or null if none is found</returns>
        string LookupNamespaceURI(string prefix);

        /// <summary>
        /// Get the prefix associated with the given namespace URI.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#Node3-lookupNamespacePrefix:
        /// "Look up the prefix associated to the given namespace URI, starting
        /// from this node. The default namespace declarations are ignored by 
        /// this method.
        /// 
        /// See Namespace Prefix Lookup for details on the algorithm used by this method.
        /// </remarks>
        /// <param name="namespaceURI">The namespace URI to look for</param>
        /// <returns>Returns an associated namespace prefix if found or null 
        /// if none is found. If more than one prefix are associated to the 
        /// namespace prefix, the returned namespace prefix is implementation 
        /// dependent.</returns>
        string LookupPrefix(string namespaceURI);
        
        /// <summary>
        /// Determine if the given namespace URI is the default namespace.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#Node3-isDefaultNamespace:
        /// "This method checks if the specified namespaceURI is the default 
        /// namespace or not."
        /// </remarks>
        /// <param name="namespaceURI">The namespace URI to look for.</param>
        /// <returns>Returns true if the specified namespaceURI is the default namespace, false otherwise.</returns>
        bool IsDefaultNamespace(string namespaceURI);
        
        /// <summary>
        /// Get this node's value (depends on the node type)
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-F68D080:
        /// "The value of this node, depending on its type... When it is 
        /// defined to be null, setting it has no effect, including if the 
        /// node is read-only.
        /// 
        ///     Type                    NodeValue
        ///     ----------              ----------
        ///     Attr                    same as Attr.value
        ///     CDATASection            same as CharacterData.data, the content of the CDATA Section
        ///     Comment                 same as CharacterData.data, the content of the comment
        ///     DocumentFragment        null
        ///     Document                null
        ///     DocumentType            null
        ///     Element                 null
        ///     Entity                  null
        ///     EntityReference         null
        ///     Notation                null
        ///     ProcessingInstruction   same as ProcessingInstruction.data
        ///     Text                    same as CharacterData.data, the content of the text node
        ///     
        /// Exceptions on setting
        ///     NO_MODIFICATION_ALLOWED_ERR: Raised when the node is readonly 
        ///     and if it is not defined to be null.
        /// 
        /// Exceptions on retrieval
        ///     DOMSTRING_SIZE_ERR: Raised when it would return more 
        ///     characters than fit in a DOMString variable on the 
        ///     implementation platform."
        ///    
        /// </remarks>
        string NodeValue { get; set; }

        /// <summary>
        /// Determine if this node has any attributes.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-NodeHasAttrs:
        /// "Returns whether this node (if it is an element) has any attributes."
        /// </remarks>
        /// <returns>Returns true if this node has any attributes, false otherwise.</returns>
        bool HasAttributes();

        /// <summary>
        /// Get this node's attribute collection.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-84CF096:
        /// "A NamedNodeMap containing the attributes of this node (if it is 
        /// an Element) or null otherwise."
        /// </remarks>
        NamedNodeMap Attributes { get; }

        /// <summary>
        /// Determine if this node has any child nodes
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-810594187:
        /// "Returns whether this node has any children."
        /// </remarks>
        /// <returns>Returns true if this node has any children, false otherwise.</returns>
        bool HasChildNodes();

        /// <summary>
        /// Get this node's collection of child nodes.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-1451460987:
        /// "A NodeList that contains all children of this node. If there are 
        /// no children, this is a NodeList containing no nodes."
        /// </remarks>
        NodeList ChildNodes { get; }

        /// <summary>
        /// Get this node's parent node (null if none)
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-1060184317:
        /// "The parent of this node. All nodes, except Attr, Document, 
        /// DocumentFragment, Entity, and Notation may have a parent. However, 
        /// if a node has just been created and not yet added to the tree, or 
        /// if it has been removed from the tree, this is null."
        /// </remarks>
        Node ParentNode { get; }

        /// <summary>
        /// Get this node's first child node (null if none).
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-169727388:
        /// "The first child of this node. If there is no such node, this returns null."
        /// </remarks>
        Node FirstChild { get; }

        /// <summary>
        /// Get this node's last child node (null if none)
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-61AD09FB:
        /// "The last child of this node. If there is no such node, this returns null."
        /// </remarks>
        Node LastChild { get; }

        /// <summary>
        /// Get this node's previous sibling node (null if none)
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-640FB3C8:
        /// "The node immediately preceding this node. If there is no such 
        /// node, this returns null."
        /// </remarks>
        Node PreviousSibling { get; }

        /// <summary>
        /// Get this node's next sibling node (null if none)
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-6AC54C2F:
        /// "The node immediately following this node. If there is no such 
        /// node, this returns null."
        /// </remarks>
        Node NextSibling { get; }

        /// <summary>
        /// Append a node to this node's child collection
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-184E7107:
        /// "Adds the node newChild to the end of the list of children of this 
        /// node. If the newChild is already in the tree, it is first removed."
        /// </remarks>
        /// <param name="newChild">The node to add. If it is a DocumentFragment 
        /// object, the entire contents of the document fragment are moved into 
        /// the child list of this node</param>
        /// <returns>The node added.</returns>
        /// <exception cref="DOMException">
        ///     HIERARCHY_REQUEST_ERR: Raised if this node is of a type that 
        ///     does not allow children of the type of the newChild node, or if
        ///     the node to append is one of this node's ancestors or this node 
        ///     itself, or if this node is of type Document and the DOM 
        ///     application attempts to append a second DocumentType or Element 
        ///     node.
        ///
        ///     WRONG_DOCUMENT_ERR: Raised if newChild was created from a 
        ///     different document than the one that created this node.
        ///
        ///     NO_MODIFICATION_ALLOWED_ERR: Raised if this node is readonly 
        ///     or if the previous parent of the node being inserted is readonly.
        ///     
        ///     NOT_SUPPORTED_ERR: if the newChild node is a child of the 
        ///     Document node, this exception might be raised if the DOM 
        ///     implementation doesn't support the removal of the DocumentType 
        ///     child or Element child.
        /// </exception>
        Node AppendChild(Node newChild);

        /// <summary>
        /// Insert a node into this node's child collection.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-952280727:
        /// "Inserts the node newChild before the existing child node refChild. 
        /// If refChild is null, insert newChild at the end of the list of 
        /// children. If newChild is a DocumentFragment object, all of its 
        /// children are inserted, in the same order, before refChild. If the 
        /// newChild is already in the tree, it is first removed.
        /// 
        /// Note: Inserting a node before itself is implementation dependent"
        /// </remarks>
        /// <param name="newChild">The node to insert.</param>
        /// <param name="refChild">The reference node, i.e., the node before 
        /// which the new node must be inserted.</param>
        /// <returns>The node being inserted.</returns>
        /// <exception cref="DOMException">
        ///     HIERARCHY_REQUEST_ERR: Raised if this node is of a type that 
        ///     does not allow children of the type of the newChild node, or if
        ///     the node to insert is one of this node's ancestors or this node 
        ///     itself, or if this node is of type Document and the DOM 
        ///     application attempts to insert a second DocumentType or Element 
        ///     node.
        ///     
        ///     WRONG_DOCUMENT_ERR: Raised if newChild was created from a 
        ///     different document than the one that created this node.
        ///     
        ///     NO_MODIFICATION_ALLOWED_ERR: Raised if this node is readonly 
        ///     or if the parent of the node being inserted is readonly.
        ///     
        ///     NOT_FOUND_ERR: Raised if refChild is not a child of this node.
        ///     
        ///     NOT_SUPPORTED_ERR: if this node is of type Document, this 
        ///     exception might be raised if the DOM implementation doesn't 
        ///     support the insertion of a DocumentType or Element node.
        /// </exception>
        Node InsertBefore(Node newChild, Node refChild);

        /// <summary>
        /// Remove a node from this node's child collection
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-1734834066:
        /// "Removes the child node indicated by oldChild from the list of 
        /// children, and returns it."
        /// </remarks>
        /// <param name="oldChild">The node being removed.</param>
        /// <returns>The node removed.</returns>
        /// <exception cref="DOMException">
        ///     NO_MODIFICATION_ALLOWED_ERR: Raised if this node is readonly.
        ///     
        ///     NOT_FOUND_ERR: Raised if oldChild is not a child of this node.
        ///     
        ///     NOT_SUPPORTED_ERR: if this node is of type Document, this 
        ///     exception might be raised if the DOM implementation doesn't 
        ///     support the removal of the DocumentType child or the Element child.
        /// </exception>
        Node RemoveChild(Node oldChild);

        /// <summary>
        /// Replace a node within this node's child collection.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-785887307:
        /// "Replaces the child node oldChild with newChild in the list of 
        /// children, and returns the oldChild node. If newChild is a 
        /// DocumentFragment object, oldChild is replaced by all of the 
        /// DocumentFragment children, which are inserted in the same order. If 
        /// the newChild is already in the tree, it is first removed.
        /// 
        /// Note: Replacing a node with itself is implementation dependent."
        /// </remarks>
        /// <param name="newChild">The new node to put in the child list.</param>
        /// <param name="oldChild">The node being replaced in the list.</param>
        /// <returns>The node replaced.</returns>
        /// <exception cref="DOMException">
        ///     HIERARCHY_REQUEST_ERR: Raised if this node is of a type that 
        ///     does not allow children of the type of the newChild node, or 
        ///     if the node to put in is one of this node's ancestors or this
        ///     node itself, or if this node is of type Document and the result 
        ///     of the replacement operation would add a second DocumentType or 
        ///     Element on the Document node.
        ///     
        ///     WRONG_DOCUMENT_ERR: Raised if newChild was created from a 
        ///     different document than the one that created this node.
        /// 
        ///     NO_MODIFICATION_ALLOWED_ERR: Raised if this node or the parent 
        ///     of the new node is readonly.
        /// 
        ///     NOT_FOUND_ERR: Raised if oldChild is not a child of this node.
        /// 
        ///     NOT_SUPPORTED_ERR: if this node is of type Document, this 
        ///     exception might be raised if the DOM implementation doesn't 
        ///     support the replacement of the DocumentType child or Element 
        ///     child.
        /// </exception>
        Node ReplaceChild(Node newChild, Node oldChild);
        
        /// <summary>
        /// Determine if the given node references the same object as the invoking reference
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#Node3-isSameNode:
        /// "Returns whether this node is the same node as the given one. This 
        /// method provides a way to determine whether two Node references 
        /// returned by the implementation reference the same object. When two 
        /// Node references are references to the same object, even if through 
        /// a proxy, the references may be used completely interchangeably, 
        /// such that all attributes have the same values and calling the same 
        /// DOM method on either reference always has exactly the same effect."
        /// </remarks>
        /// <param name="other">The node to test against.</param>
        /// <returns>Returns true if the nodes are the same, false otherwise.</returns>
        bool IsSameNode(Node other);

        /// <summary>
        /// Determine if the given node is equivalent to the invoking node.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#Node3-isEqualNode:
        /// "Tests whether two nodes are equal. This method tests for equality 
        /// of nodes, not sameness (i.e., whether the two nodes are references
        /// to the same object) which can be tested with Node.isSameNode(). 
        /// All nodes that are the same will also be equal, though the reverse 
        /// may not be true.
        /// 
        /// Two nodes are equal if and only if the following conditions are 
        /// satisfied:
        /// 
        /// * The two nodes are of the same type.
        /// * The following string attributes are equal: nodeName, localName, 
        ///   namespaceURI, prefix, nodeValue. This is: they are both null, or 
        ///   they have the same length and are character for character 
        ///   identical.
        /// * The attributes NamedNodeMaps are equal. This is: they are both 
        ///   null, or they have the same length and for each node that exists
        ///   in one map there is a node that exists in the other map and is 
        ///   equal, although not necessarily at the same index.
        /// * The childNodes NodeLists are equal. This is: they are both null, 
        ///   or they have the same length and contain equal nodes at the same 
        ///   index. Note that normalization can affect equality; to avoid 
        ///   this, nodes should be normalized before being compared.
        ///   
        /// For two DocumentType nodes to be equal, the following conditions 
        /// must also be satisfied:
        /// 
        /// * The following string attributes are equal: publicId, systemId, 
        ///   internalSubset.
        /// * The entities NamedNodeMaps are equal.
        /// * The notations NamedNodeMaps are equal.
        /// 
        /// On the other hand, the following do not affect equality: the 
        /// ownerDocument, baseURI, and parentNode attributes, the specified 
        /// attribute for Attr nodes, the schemaTypeInfo attribute for Attr 
        /// and Element nodes, the Text.isElementContentWhitespace attribute 
        /// for Text nodes, as well as any user data or event listeners 
        /// registered on the nodes.
        /// 
        /// Note: As a general rule, anything not mentioned in the description 
        /// above is not significant in consideration of equality checking.
        /// Note that future versions of this specification may take into 
        /// account more attributes and implementations conform to this 
        /// specification are expected to be updated accordingly."
        /// </remarks>
        /// <param name="other">The node to compare equality with.</param>
        /// <returns>Returns true if the nodes are equal, false otherwise.</returns>
        bool IsEqualNode(Node other);

        /// <summary>
        /// Determine the position of a node relative to the invoking node.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#Node3-compareDocumentPosition:
        /// "Compares the reference node, i.e. the node on which this method 
        /// is being called, with a node, i.e. the one passed as a parameter, 
        /// with regard to their position in the document and according to
        /// the document order."
        /// </remarks>
        /// <param name="other">The node to compare against the reference node.</param>
        /// <returns>Returns how the node is positioned relatively to the reference node.</returns>
        /// <exception cref="DOMException">
        /// NOT_SUPPORTED_ERR: when the compared nodes are from different DOM
        /// implementations that do not coordinate to return consistent 
        /// implementation-specific results.
        /// </exception>
        DOMDocumentPosition CompareDocumentPosition(Node other);

        /// <summary>
        /// Create a clone of this node
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-3A0ED0A4:
        /// "Returns a duplicate of this node, i.e., serves as a generic copy
        /// constructor for nodes. The duplicate node has no parent 
        /// (parentNode is null) and no user data. User data associated to the 
        /// imported node is not carried over. However, if any 
        /// UserDataHandlers has been specified along with the associated data
        /// these handlers will be called with the appropriate parameters before 
        /// this method returns.
        /// 
        /// Cloning an Element copies all attributes and their values, including 
        /// those generated by the XML processor to represent defaulted attributes, 
        /// but this method does not copy any children it contains unless it is a 
        /// deep clone. This includes text contained in an the Element since the 
        /// text is contained in a child Text node. Cloning an Attr directly, as
        /// opposed to be cloned as part of an Element cloning operation, returns 
        /// a specified attribute (specified is true). Cloning an Attr always 
        /// clones its children, since they represent its value, no matter whether 
        /// this is a deep clone or not. Cloning an EntityReference automatically
        /// constructs its subtree if a corresponding Entity is available, no 
        /// matter whether this is a deep clone or not. Cloning any other type of 
        /// node simply returns a copy of this node.
        /// 
        /// Note that cloning an immutable subtree results in a mutable copy, but 
        /// the children of an EntityReference clone are readonly. In addition, 
        /// clones of unspecified Attr nodes are specified. And, cloning Document, 
        /// DocumentType, Entity, and Notation nodes is implementation dependent."
        /// </remarks>
        /// <param name="deep">If true, recursively clone the subtree under the 
        /// specified node; if false, clone only the node itself (and its 
        /// attributes, if it is an Element).</param>
        /// <returns>The duplicate node.</returns>
        Node CloneNode(bool deep);

        /// <summary>
        /// Normalize the text content of this node.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#ID-normalize:
        /// "Puts all Text nodes in the full depth of the sub-tree underneath
        /// this Node, including attribute nodes, into a "normal" form where 
        /// only structure (e.g., elements, comments, processing instructions, 
        /// CDATA sections, and entity references) separates Text nodes, i.e.,
        /// there are neither adjacent Text nodes nor empty Text nodes. This 
        /// can be used to ensure that the DOM view of a document is the same
        /// as if it were saved and re-loaded, and is useful when operations 
        /// (such as XPointer [XPointer] lookups) that depend on a particular 
        /// document tree structure are to be used. If the parameter 
        /// "normalize-characters" of the DOMConfiguration object attached to 
        /// the Node.ownerDocument is true, this method will also fully 
        /// normalize the characters of the Text nodes.
        /// 
        /// Note: In cases where the document contains CDATASections, the 
        /// normalize operation alone may not be sufficient, since XPointers 
        /// do not differentiate between Text nodes and CDATASection nodes."
        /// </remarks>
        void Normalize();

        /// <summary>
        /// Determine if this node supports the specified feature.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#Level-2-Core-Node-supports:
        /// "Tests whether the DOM implementation implements a specific feature 
        /// and that feature is supported by this node, as specified in DOM 
        /// Features."
        /// </remarks>
        /// <param name="feature">The name of the feature to test. </param>
        /// <param name="version">This is the version number of the feature to test. </param>
        /// <returns>Returns true if the specified feature is supported on this node, false otherwise.</returns>
        bool IsSupported(string feature, string version);

        /// <summary>
        /// Get an interface implementing the specified feature.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#Node3-getFeature:
        /// "This method returns a specialized object which implements the
        /// specialized APIs of the specified feature and version, as specified 
        /// in DOM Features. The specialized object may also be obtained by 
        /// using binding-specific casting methods but is not necessarily 
        /// expected to, as discussed in Mixed DOM Implementations. This method 
        /// also allow the implementation to provide specialized objects which 
        /// do not support the Node interface."
        /// </remarks>
        /// <param name="feature">The name of the feature requested. Note that 
        /// any plus sign "+" prepended to the name of the feature will be 
        /// ignored since it is not significant in the context of this method.</param>
        /// <param name="version">This is the version number of the feature to test. </param>
        /// <returns>Returns an object which implements the specialized APIs of 
        /// the specified feature and version, if any, or null if there is no 
        /// object which implements interfaces associated with that feature. 
        /// If the DOMObject returned by this method implements the Node 
        /// interface, it must delegate to the primary core Node and not
        /// return results inconsistent with the primary core Node such as 
        /// attributes, childNodes, etc.</returns>
        object GetFeature(string feature, string version);

        /// <summary>
        /// Get or set the text content of this node.
        /// </summary>
        /// <remarks>
        /// http://www.w3.org/TR/2004/REC-DOM-Level-3-Core-20040407/core.html#Node3-textContent:
        /// This attribute returns the text content of this node and its 
        /// descendants. When it is defined to be null, setting it has no 
        /// effect. On setting, any possible children this node may have are 
        /// removed and, if it the new string is not empty or null, replaced 
        /// by a single Text node containing the string this attribute is set 
        /// to. 
        /// 
        /// On getting, no serialization is performed, the returned string does 
        /// not contain any markup. No whitespace normalization is performed and 
        /// the returned string does not contain the white spaces in element 
        /// content (see the attribute Text.isElementContentWhitespace). 
        /// Similarly, on setting, no parsing is performed either, the input 
        /// string is taken as pure textual content. 
        /// 
        /// The string returned is made of the text content of this node 
        /// depending on its type, as defined below:
        /// 
        /// ELEMENT_NODE, 
        /// ATTRIBUTE_NODE, 
        /// ENTITY_NODE, 
        /// ENTITY_REFERENCE_NODE, 
        /// DOCUMENT_FRAGMENT_NODE:
        ///     Returns the concatenation of the textContent attribute value 
        ///     of every child node, excluding COMMENT_NODE and 
        ///     PROCESSING_INSTRUCTION_NODE nodes. This is the empty string 
        ///     if the node has no children.
        /// TEXT_NODE, 
        /// CDATA_SECTION_NODE, 
        /// COMMENT_NODE, 
        /// PROCESSING_INSTRUCTION_NODE:
        ///     Returns NodeValue
        /// DOCUMENT_NODE, 
        /// DOCUMENT_TYPE_NODE, 
        /// NOTATION_NODE:
        ///     Returns null"
        /// </remarks>
        string TextContent { get; set; }

        object SetUserData(string key, object data, DOMUserDataHandler handler);
        object GetUserData(string key);
        string InnerText { get; set; }
        string BaseURI { get; }
    }
}
